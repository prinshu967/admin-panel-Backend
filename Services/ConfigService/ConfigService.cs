using AngularAdminPannel.Data;
using AngularAdminPannel.DTOs;
using AngularAdminPannel.DTOs.ApplicationConfigration;
using AngularAdminPannel.Models;
using AngularAdminPannel.Services.FAQService;
using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AngularAdminPannel.Services.ConfigService
{
    public class ConfigService :IConfigService
    {
        private readonly ApplicationDbContext _context;
        private readonly ClaimsPrincipal? _user;

        public ConfigService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _user = httpContextAccessor.HttpContext?.User;
        }
        public async Task<PagedResult<ConfigListItemDto>> GetConfigsAsync(ConfigListFilterDto filter)
        {
            var pageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
            var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;
            var query = _context.Configs.AsNoTracking();
            bool hasCustomOrder = false;


            //if (!string.IsNullOrEmpty(filter.Search))
            //{
            //    var search = filter.Search.Trim();
            //    query = query.Where(cms => cms.Title!.Contains(search));
            //}



            if (!string.IsNullOrWhiteSpace(filter.Filter) && !string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();
                var searchUpper = search.ToUpperInvariant();
                var filterType = filter.Filter.Trim().ToUpperInvariant();


                if (filterType == "KEY")
                {
                    query = query.Where(u => u.Key!.StartsWith(searchUpper));
                }
                else if (filterType == "VALUE")
                {
                    query = query.Where(u => (u.Value ?? "").StartsWith(searchUpper));
                }
                else if (filterType == "DISPLAYORDER")
                {
                    query = query.Where(u => u.DisplayOrder.ToString().StartsWith(searchUpper));
                }
                else if (filterType == "ORDERKEY")
                {
                    if (searchUpper == "ASC")
                    {
                        query = query.OrderBy(u => u.Key);
                        hasCustomOrder = true;

                    }
                    else
                    {
                        query = query.OrderByDescending(u => u.Key);
                        hasCustomOrder = true;

                    }
                }
                else if (filterType == "ORDERVALUE")
                {
                    if (searchUpper == "ASC")
                    {
                        query = query.OrderBy(u => u.Value);
                        hasCustomOrder = true;

                    }
                    else
                    {
                        query = query.OrderByDescending(u => u.Value);
                        hasCustomOrder = true;

                    }
                }
                else if (filterType == "ORDERDISPLAYORDER")
                {
                    if (searchUpper == "ASC")
                    {
                        query = query.OrderBy(u => u.DisplayOrder);
                        hasCustomOrder = true;

                    }
                    else
                    {
                        query = query.OrderByDescending(u => u.DisplayOrder);
                        hasCustomOrder = true;

                    }
                }

            }










            if (filter.IsActive.HasValue)
            {
                query = query.Where(r => r.IsActive == filter.IsActive.Value);
            }
            if (!hasCustomOrder)
            {
                query = query
                    .OrderByDescending(u => u.CreatedOn);

            }
            var totalCount = await query.CountAsync();

            var faqList = await query
                
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(r => new ConfigListItemDto
                {
                    Id = r.Id,
                    Key = r.Key,
                    Value = r.Value,
                    IsActive = r.IsActive,
                    DisplayOrder = r.DisplayOrder,

                })
                .ToListAsync();
            return new PagedResult<ConfigListItemDto>
            {
                Items = faqList,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize

            };
        }


        public async Task<(IdentityResult Result, Guid? RoleId)> CreateAsync(ConfigCreateDto model)
        {


            if (await _context.Configs.AnyAsync(r => r.Key == model.Key))
            {
                return (IdentityResult.Failed(new IdentityError { Description = "Config with this Key already exists." }), null);
            }
           
            var idString = _user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Safely parse string to Guid — fallback to Guid.Empty
            var _userId = Guid.TryParse(idString, out var parsedGuid) ? parsedGuid : Guid.Empty;

            var newFAQ = new ApplicationConfig
            {
                Id = Guid.NewGuid(),
                Key = model.Key,
                Value = model.Value,
                DisplayOrder = model.DisplayOrder,
                IsActive = model.IsActive,


                CreatedOn = DateTime.UtcNow,
                CreatedBy =_userId, // Replace with actual user id if available
                ModifiedBy = _userId,
                ModifiedOn = DateTime.UtcNow,

            };

            // Add to context
            await _context.Configs.AddAsync(newFAQ);

            // Save changes to the database
            var result = await _context.SaveChangesAsync();

            // Check if the save was successful
            if (result > 0)
            {
                return (IdentityResult.Success, newFAQ.Id);
            }

            // If save failed, return a failure result
            return (IdentityResult.Failed(new IdentityError { Description = "An error occurred while saving the FAQ record." }), null);
        }


        public async Task<ConfigEditDto?> GetForEditAsync(Guid id)
        {
            var faq = await _context.Configs.FirstOrDefaultAsync(x => x.Id == id);
            if (faq == null)
            {
                return null;

            }
            return new ConfigEditDto
            {
                Id = faq.Id,
                Key=faq.Key,
                Value=faq.Value,
                DisplayOrder = faq.DisplayOrder,
                IsActive = faq.IsActive,



            };

        }


        public async Task<IdentityResult> UpdateAsync(ConfigEditDto model)
        {
            if (await _context.Configs.AnyAsync(r => r.Key == model.Key.Trim()&&r.Id!=model.Id))
            {
                return (IdentityResult.Failed(new IdentityError { Description = "Config with this Key already exists." }));
            }

            // Find the existing CMS record by Id
            var faq = await _context.Configs.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (faq == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "NotFound",
                    Description = "FAQ Not found."
                });
            }
            
           


            var idString = _user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Safely parse string to Guid — fallback to Guid.Empty
            var _userId = Guid.TryParse(idString, out var parsedGuid) ? parsedGuid : Guid.Empty;





            // Update fields
            faq.Key = model.Key;
            faq.Value = model.Value;
            faq.DisplayOrder = model.DisplayOrder;



            faq.IsActive = model.IsActive;
            faq.ModifiedOn = DateTime.UtcNow;
            faq.ModifiedBy = _userId;

            // Save changes
            var result = await _context.SaveChangesAsync();

            // Return success/failure
            if (result > 0)
                return IdentityResult.Success;

            return IdentityResult.Failed(new IdentityError
            {
                Code = "UpdateFailed",
                Description = "An error occurred while updating the FAQ record."
            });
        }


        public async Task<IdentityResult> ToggleActive(Guid id)
        {
            var config = await _context.Configs.FirstOrDefaultAsync(x => x.Id == id);
            if (config == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "NotFound",
                    Description = "Config not found."
                });
            }

            config.IsActive = !config.IsActive;

            try
            {
                await _context.SaveChangesAsync();
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                // Optionally log the exception
                Console.WriteLine(ex.Message);
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "SaveFailed",
                    Description = "Could not update Config."
                });
            }
        }


        public async Task<IdentityResult> DeleteAsync(Guid id)
        {
            // Find the CMS page by Id
            var faq = await _context.Configs.FirstOrDefaultAsync(x => x.Id == id);
            if (faq == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "NotFound",
                    Description = "FAQ not found."
                });
            }

            // Remove the record
            _context.Configs.Remove(faq);

            // Save changes
            var result = await _context.SaveChangesAsync();

            // Return success/failure
            if (result > 0)
            {
                return IdentityResult.Success;
            }

            return IdentityResult.Failed(new IdentityError
            {
                Code = "DeleteFailed",
                Description = "An error occurred while deleting the FAQ record."
            });
        }








    }
}
