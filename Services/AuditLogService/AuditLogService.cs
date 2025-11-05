using AngularAdminPannel.Data;
using AngularAdminPannel.DTOs;
using AngularAdminPannel.DTOs.AuditLog;
using AngularAdminPannel.DTOs.Roles;
using AngularAdminPannel.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AngularAdminPannel.Services.AuditLogService
{
    public class AuditLogService:IAuditLogService
    {
        private readonly ApplicationDbContext _context;
        public AuditLogService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<PagedResult<AuditLogListItemDto>> GetAuditAsync(AuditLogFilterDto filterDto)
        {
            var pageNumber = filterDto.PageNumber <= 0 ? 1 : filterDto.PageNumber;
            var pageSize = filterDto.PageSize <= 0 ? 10 : filterDto.PageSize;
            bool hasCustomOrder = false;

            var query = _context.AuditLogs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filterDto.Filter) && !string.IsNullOrWhiteSpace(filterDto.Search))
            {
                var search = filterDto.Search.Trim();
                var searchUpper = search.ToUpperInvariant();
                var filterType = filterDto.Filter.Trim().ToUpperInvariant();

                if (filterType == "NAME")
                {
                    query = query.Where(u => (u.UserName ?? "").ToUpper().Contains(searchUpper));
                }
                else if (filterType == "TYPE")
                {
                    query = query.Where(u => (u.Type ?? "").ToUpper().Contains(searchUpper));
                }

                else if (filterType == "ACTIVITY")
                {
                    query = query.Where(u => (u.Activity?? "").ToUpper().Contains(searchUpper));
                }
                else if (filterType == "ORDERNAME")
                {
                    if (searchUpper == "ASC")
                    {
                        query = query.OrderBy(u => u.UserName);
                        hasCustomOrder = true;

                    }
                    else
                    {
                        query = query.OrderByDescending(u => u.UserName);
                        hasCustomOrder = true;

                    }
                }
                else if (filterType == "ORDERTYPE")
                {
                    if (searchUpper == "ASC")
                    {
                        query = query.OrderBy(u => u.Type);
                        hasCustomOrder = true;

                    }
                    else
                    {
                        query = query.OrderByDescending(u => u.Type);
                        hasCustomOrder = true;

                    }
                }
                else if (filterType == "ORDERACTIVITY")
                {
                    if (searchUpper == "ASC")
                    {
                        query = query.OrderBy(u => u.Activity);
                        hasCustomOrder = true;

                    }
                    else
                    {
                        query = query.OrderByDescending(u => u.Activity);
                        hasCustomOrder = true;

                    }
                }
                else if (filterType == "ORDERDATE")
                {
                    if (searchUpper == "ASC")
                    {
                        query = query.OrderBy(u => u.CreatedOn);
                        hasCustomOrder = true;

                    }
                    else
                    {
                        query = query.OrderByDescending(u => u.CreatedOn);
                        hasCustomOrder = true;

                    }
                }
                else
                {
                    DateTime searchDate = DateTime.Parse(searchUpper).Date;

                    // Filter by range to ensure it translates to SQL
                    query = query.Where(u => u.CreatedOn >= searchDate
                                                 && u.CreatedOn < searchDate.AddDays(1));

                }
            }
            if (!hasCustomOrder)
            {
                query = query
                    .OrderByDescending(u => u.CreatedOn);

            }

            var totalCount = await query.CountAsync();

            var logs = await query
               
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new AuditLogListItemDto
                {
                    Id = r.Id,
                    UserName = r.UserName,
                    Type = r.Type,
                    Activity = r.Activity,
                    CreatedOn = r.CreatedOn.AddHours(5).AddMinutes(30),
                })
                .ToListAsync();

            return new PagedResult<AuditLogListItemDto>
            {
                Items = logs,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
            };
        }


        public async Task LogAsync(string userName, string type, string activity)
        {
            var log = new AuditLog
            {
                UserName = userName,
                Type = type,
                Activity = activity,
               
                CreatedOn = DateTime.UtcNow
            };

            await _context.AuditLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

    }
}
