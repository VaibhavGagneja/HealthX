using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataService.Data;
using DataService.IConfiguration;
using Entities.DbSet;
using Entities.Dtos.Incoming;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApiWithAuth.Controllers.v1
{
    [Authorize(AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme)]
    public class UsersController : BaseController
    {
        //private AppDbContext _context;

        //public UsersController(AppDbContext context){
        //    _context = context;
        //}

        public UsersController(IUnitOfWork unitOfWork):base(unitOfWork) { }

        [HttpGet]
        [HttpHead]
        public async Task<IActionResult> Getusers()
        {
            var users = await _unitOfWork.Users.All();
            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(UserDto user)
        {
            var _user = new User();
            _user.LastName = user.LastName;
            _user.FirstName = user.FirstName;
            _user.Email = user.Email;
            _user.DateOfBirth = Convert.ToDateTime(user.DateOfBirth);
            _user.Country = user.Country;
            _user.Status = 1;

            await _unitOfWork.Users.Add(_user);
            await _unitOfWork.CompletedAsync();
            //_context.Users.Add(_user);
            //_context.SaveChanges();
            return CreatedAtRoute("GetUser", new { id = _user.Id }, user);
        }

        [HttpGet]
        [Route("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            //var user = _context.Users.FirstOrDefault(x => x.Id==id);   
            var user = await _unitOfWork.Users.GetById(id);
            return Ok(user);
        }
    }
}