using ADSBackend.Data;
using ADSBackend.Models;
using ADSBackend.Models.ApiModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ADSBackend.Controllers
{
    [Produces("application/json")]
    [Route("api")]
    public class ApiController : Controller
    {
        private readonly Services.Configuration Configuration;
        private readonly Services.Cache _cache;
        private readonly ApplicationDbContext _context;

        public ApiController(Services.Configuration configuration, Services.Cache cache, ApplicationDbContext context)
        {
            Configuration = configuration;
            _cache = cache;
            _context = context;
        }


        // GET: api/News
        [HttpGet("Meetings")]
        public async Task<List<Meeting>> GetMeetings()
        {
            return await _context.Meeting.OrderByDescending(x => x.Start).ToListAsync();
        }

        [HttpPost("Login")]
        public async Task<object> Login(IFormCollection forms)
        {
            string _email, _password;

            if (!forms.ContainsKey("email") &&
                !forms.ContainsKey("password"))
                return new
                {
                    Status = "Invalid form data"
                };

            // Get fields from forms
            _email = forms["email"];
            _password = forms["password"];

            // Look up username and password in _context.Member
            Member member = await _context.Member.FirstOrDefaultAsync(m => m.Email == _email && m.Password == _password);

            if (member == null)
            {
                // Member email or password is incorrect
                return new
                {
                    Status = "InvalidCredentials"
                };
            }

            var sessionKey = System.Guid.NewGuid().ToString();
            var session = await _context.Session?.FirstOrDefaultAsync(s => s.MemberId == member.MemberId);

            if (session != null)
            {
                sessionKey = session.Key;
                session.LastAccessTime = DateTime.Now;
                _context.Update(session);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Create a new session
                session = new Session
                {
                    MemberId = member.MemberId,
                    Key = sessionKey,
                    Email = member.Email,
                    LastAccessTime = DateTime.Now
                };

                _context.Add(session);
                await _context.SaveChangesAsync();
            }

            var response = new
            {
                Status = "LoggedIn",
                Key = sessionKey
            };

            return response;
        }

        // GET: api/Config
        [HttpGet("Config")]
        public ConfigResponse GetConfig()
        {
            // TODO: extend this object to include some configuration items
            return new ConfigResponse();
        }
    }
}