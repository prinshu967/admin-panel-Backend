using AngularAdminPannel.Data;
using AngularAdminPannel.DTOs;
using AngularAdminPannel.DTOs.Dashboard;
using AngularAdminPannel.DTOs.Users;
using AngularAdminPannel.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AngularAdminPannel.Services.DashboardService
{
    public class DashboardService : IDashboardService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _context;


        public DashboardService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager,ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }

        public async Task<DashboardResponseDto> GetAsync()
        {
            // Use async EF methods for better performance and scalability
            var totalUsers = await _userManager.Users
                .AsNoTracking()
                .CountAsync();

            var totalRoles = await _roleManager.Roles
                .AsNoTracking()
                .CountAsync();


            var result = await _context.AuditLogs
            .GroupBy(a => new { a.UserName, a.Type, a.Activity })
            .Select(g => new DashboardInsightDto
            {
                UserName = g.Key.UserName,
                Type = g.Key.Type,
                Activity = g.Key.Activity,
                ActivityCount = g.Count()
            })
            .OrderBy(x => x.UserName)
            .ThenBy(x => x.Type)
            .ThenBy(x => x.Activity)
            .ToListAsync();


            return new DashboardResponseDto
            {
                TotalUsers = totalUsers,
                TotalRoles = totalRoles,
                DashboardInsights=result
            };
        }
    }
}
