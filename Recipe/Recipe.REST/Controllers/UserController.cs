﻿using AutoMapper;
using Firebase.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Recipe.Auth.Models;
using Recipe.Auth.ModelsCommon;
using Recipe.ExceptionHandler.CustomExceptions;
using Recipe.REST.ViewModels.User;
using Recipe.Service.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recipe.REST.Controllers
{
    [Route("api/user")] //NOTE: You can also use this api/[controller]/[action]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        [HttpPost("/user/register")]
        public async Task<IActionResult> Register(UserRegisterVM registerModel)
        {
            try
            {
                FirebaseAuthLink UserInfo = await _userService.Register(_mapper.Map<AuthUser>(registerModel));
                string Token = UserInfo.FirebaseToken;
                string RefreshToken = UserInfo.RefreshToken;

                //saving the token in a session variable
                if (Token != null)
                {
                    HttpContext.Session.SetString("_UserToken", Token);
                    HttpContext.Session.SetString("_UserRefreshToken", RefreshToken);

                    return Ok(_mapper.Map<UserReturnVM>(UserInfo));
                }

                throw new HttpStatusCodeException(StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost("/user/login")]
        public async Task<IActionResult> Login(UserLoginVM loginModel)
        {
            try
            {
                //log in an existing user
                FirebaseAuthLink UserInfo = await _userService.Login(_mapper.Map<AuthUser>(loginModel));
                string Token = UserInfo.FirebaseToken;
                string RefreshToken = UserInfo.RefreshToken;

                //saving the token in a session variable
                if (Token != null)
                {
                    HttpContext.Session.SetString("_UserToken", Token);
                    HttpContext.Session.SetString("_UserRefreshToken", RefreshToken);

                    return Ok(_mapper.Map<UserReturnVM>(UserInfo));
                }

                throw new HttpStatusCodeException(StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost("/user/logout"), Authorize]
        public IActionResult LogOut()
        {
            try
            {
                HttpContext.Session.Remove("_UserToken");
                return Ok();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost("/refreshToken/{userId}"), Authorize]
        public async Task<IActionResult> RefreshToken(string userId)
        {
            try
            {
                await _userService.RefreshToken(userId);

                return Ok();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet("/user/{userId}"), Authorize]
        public async Task<IActionResult> GetByID(string userId)
        {
            try
            {
                IAuthUser result = await _userService.GetByIDAsync(userId);

                if (result == null)
                    throw new HttpStatusCodeException(StatusCodes.Status204NoContent, $"There is no user with id {userId}.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet("/user/"), Authorize]
        public async Task<IActionResult> GetWithToken([FromQuery]string token)
        {
            try
            {
                IAuthUser result = await _userService.GetWithToken(token);

                if (result == null)
                    throw new HttpStatusCodeException(StatusCodes.Status204NoContent, $"User does not exist.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpDelete("{userId}"), Authorize]
        public async Task<IActionResult> Delete(string userId)
        {
            try
            {
                await _userService.DeleteAsync(userId);

                return Ok();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPut("{userId}"), Authorize]
        public async Task<IActionResult> Put(string userId, AuthUser user)
        {
            try
            {
                if (!ModelState.IsValid)
                    throw new HttpStatusCodeException(StatusCodes.Status400BadRequest, user);

                await _userService.UpdateAsync(userId, user);

                user.Password = null;

                return Ok(user);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
