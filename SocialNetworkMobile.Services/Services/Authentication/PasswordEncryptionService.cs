using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocialNetworkMobile.Services.Interfaces;

namespace SocialNetworkMobile.Services.Services.Authentication
{
    public class PasswordEncryptionService : IPasswordEncryptionService
    {
        private readonly IPasswordHasher<object> _passwordHasher;

        public PasswordEncryptionService()
        {
            _passwordHasher = new PasswordHasher<object>();
        }

        /// <summary>
        /// Mã hóa mật khẩu người dùng bằng thuật toán ASP.NET Core Identity.
        /// </summary>
        /// <param name="plainPassword">Mật khẩu gốc.</param>
        /// <returns>Mật khẩu đã được mã hóa.</returns>
        public string EncryptPassword(string plainPassword)
        {
            return _passwordHasher.HashPassword(null, plainPassword);
        }

        /// <summary>
        /// Xác minh mật khẩu người dùng.
        /// </summary>
        /// <param name="plainPassword">Mật khẩu gốc do người dùng nhập.</param>
        /// <param name="hashedPassword">Mật khẩu đã mã hóa được lưu trong cơ sở dữ liệu.</param>
        /// <returns>True nếu mật khẩu khớp, ngược lại là false.</returns>
        public bool VerifyPassword(string plainPassword, string hashedPassword)
        {
            var result = _passwordHasher.VerifyHashedPassword(null, hashedPassword, plainPassword);
            return result == PasswordVerificationResult.Success;
        }
    }
}
