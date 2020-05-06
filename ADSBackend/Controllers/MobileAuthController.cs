using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ADSBackend.Data;
using ADSBackend.Models;
using ADSBackend.Util;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ADSBackend.Controllers
{
    [Route("mobileauth")]
    [ApiController]
    public class AuthController : Controller
    {
        const string callbackScheme = "fblanavigator";
        private readonly ApplicationDbContext _context;
        private static Random random = new Random();

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{scheme}")]
        public async Task Get([FromRoute]string scheme)
        {
            var auth = await Request.HttpContext.AuthenticateAsync(scheme);
            if (!auth.Succeeded
                || auth?.Principal == null
                || !auth.Principal.Identities.Any(id => id.IsAuthenticated)
                || string.IsNullOrEmpty(auth.Properties.GetTokenValue("access_token")))
            {
                // Not authenticated, challenge
                await Request.HttpContext.ChallengeAsync(scheme);
            }
            else
            {
                // Get parameters to send back to the callback
                var qs = new Dictionary<string, string>
                {
                    { "access_token", auth.Properties.GetTokenValue("access_token") },
                    { "refresh_token", auth.Properties.GetTokenValue("refresh_token") ?? string.Empty },
                    { "expires", (auth.Properties.ExpiresUtc?.ToUnixTimeSeconds() ?? -1).ToString() }
                };

                var claims = auth.Principal.Claims.ToList();
                string _email = claims.FirstOrDefault(claim => claim.Type.Contains("claims/emailaddress"))?.Value;
                string _firstname = claims.FirstOrDefault(claim => claim.Type.Contains("claims/givenname"))?.Value;
                string _lastname = claims.FirstOrDefault(claim => claim.Type.Contains("claims/surname"))?.Value;
                string _fullname = claims.FirstOrDefault(claim => claim.Type.Contains("claims/name"))?.Value;

                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                string _password = new string(Enumerable.Repeat(chars, 30)
                  .Select(s => s[random.Next(s.Length)]).ToArray());
                PasswordHash ph = PasswordHasher.Hash(_password);

                Member member = new Member
                {
                    Email = _email,
                    FirstName = _firstname,
                    LastName = _lastname,
                    FullName = _fullname,
                    Password = ph.HashedPassword,
                    Salt = ph.Salt
                };
                _context.Add(member);
                await _context.SaveChangesAsync();

                Session session = new Session
                {
                    MemberId = member.MemberId,
                    Key = auth.Properties.GetTokenValue("access_token"),
                    Email = member.Email,
                    LastAccessTime = DateTime.Now
                };
                _context.Add(session);
                await _context.SaveChangesAsync();

                // Build the result url
                var url = callbackScheme + "://#" + string.Join(
                    "&",
                    qs.Where(kvp => !string.IsNullOrEmpty(kvp.Value) && kvp.Value != "-1")
                    .Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));

                // Redirect to final url
                Request.HttpContext.Response.Redirect(url);
            }
        }
    }
}