
using InvestorDashboard.Backend.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Core.Interfaces
{
    public interface IAccountManager
    {

        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
        Task<Tuple<bool, string[]>> CreateUserAsync(ApplicationUser user, string password);
        Task<Tuple<bool, string[]>> DeleteUserAsync(ApplicationUser user);
        Task<Tuple<bool, string[]>> DeleteUserAsync(string userId);
        Task<Tuple<ApplicationUser, string[]>> GetUserAndRolesAsync(string userId);
        Task<ApplicationUser> GetUserByEmailAsync(string email);
        Task<ApplicationUser> GetUserByIdAsync(string userId);
        Task<ApplicationUser> GetUserByUserNameAsync(string userName);
        Task<List<Tuple<ApplicationUser, string[]>>> GetUsersAndRolesAsync(int page, int pageSize);
        Task<Tuple<bool, string[]>> ResetPasswordAsync(ApplicationUser user, string newPassword);
        Task<Tuple<bool, string[]>> UpdatePasswordAsync(ApplicationUser user, string currentPassword, string newPassword);
        Task<Tuple<bool, string[]>> UpdateUserAsync(ApplicationUser user);
    }
}
