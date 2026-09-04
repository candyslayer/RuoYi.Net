using RuoYi.Common.Data;
using RuoYi.Common.Enums;
using RuoYi.Common.Files;
using RuoYi.Common.Utils;
using RuoYi.Data.Dtos;
using RuoYi.Data.Models;
using RuoYi.System.Services;

namespace RuoYi.Admin.Endpoints;

public static class ProfileEndpoints
{
    public static IEndpointRouteBuilder MapProfileEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/system/user/profile").RequireAuthorization();

        group.MapGet("", GetProfile);
        group.MapPut("", UpdateProfileAsync);
        group.MapPut("/updatePwd", UpdatePwdAsync);
        group.MapPost("/avatar", UploadAvatarAsync);

        return endpoints;
    }

    private static AjaxResult GetProfile(SysUserService userService)
    {
        LoginUser loginUser = SecurityUtils.GetLoginUser();
        SysUserDto user = loginUser.User;
        AjaxResult ajax = AjaxResult.Success(user);
        ajax.Add("roleGroup", userService.SelectUserRoleGroup(loginUser.UserName));
        ajax.Add("postGroup", userService.SelectUserPostGroup(loginUser.UserName));
        return ajax;
    }

    private static async Task<AjaxResult> UpdateProfileAsync(
        SysUserDto user,
        SysUserService userService,
        TokenService tokenService)
    {
        LoginUser loginUser = SecurityUtils.GetLoginUser();
        SysUserDto currentUser = loginUser.User;
        currentUser.NickName = user.NickName;
        currentUser.Email = user.Email;
        currentUser.Phonenumber = user.Phonenumber;
        currentUser.Sex = user.Sex;

        if (StringUtils.IsNotEmpty(user.Phonenumber) && !await userService.CheckPhoneUniqueAsync(currentUser))
        {
            return AjaxResult.Error("修改用户'" + user.UserName + "'失败，手机号码已存在");
        }

        if (StringUtils.IsNotEmpty(user.Email) && !await userService.CheckEmailUniqueAsync(currentUser))
        {
            return AjaxResult.Error("修改用户'" + user.UserName + "'失败，邮箱账号已存在");
        }

        if (await userService.UpdateUserProfileAsync(currentUser) > 0)
        {
            tokenService.SetLoginUser(loginUser);
            return AjaxResult.Success();
        }

        return AjaxResult.Error("修改个人信息异常，请联系管理员");
    }

    private static async Task<AjaxResult> UpdatePwdAsync(
        string oldPassword,
        string newPassword,
        SysUserService userService,
        TokenService tokenService)
    {
        LoginUser loginUser = SecurityUtils.GetLoginUser();
        string userName = loginUser.UserName;
        string password = loginUser.Password;

        if (!SecurityUtils.MatchesPassword(oldPassword, password))
        {
            return AjaxResult.Error("修改密码失败，旧密码错误");
        }

        if (SecurityUtils.MatchesPassword(newPassword, password))
        {
            return AjaxResult.Error("新密码不能与旧密码相同");
        }

        if (await userService.ResetUserPwdAsync(userName, SecurityUtils.EncryptPassword(newPassword)) > 0)
        {
            loginUser.User.Password = SecurityUtils.EncryptPassword(newPassword);
            tokenService.SetLoginUser(loginUser);
            return AjaxResult.Success();
        }

        return AjaxResult.Error("修改密码异常，请联系管理员");
    }

    private static async Task<object> UploadAvatarAsync(
        IFormFile avatarfile,
        SysUserService userService,
        TokenService tokenService)
    {
        if (avatarfile != null)
        {
            LoginUser loginUser = SecurityUtils.GetLoginUser();
            string avatar = await FileUploadUtils.UploadAsync(
                avatarfile,
                RyApp.RuoYiConfig.AvatarPath,
                MimeTypeUtils.IMAGE_EXTENSION);

            if (await userService.UpdateUserAvatar(loginUser.UserName, avatar))
            {
                loginUser.User.Avatar = avatar;
                tokenService.SetLoginUser(loginUser);

                AjaxResult ajax = AjaxResult.Success();
                ajax.Add("imgUrl", avatar);
                return ajax;
            }
        }

        return AjaxResult.Error("上传图片异常，请联系管理员");
    }
}
