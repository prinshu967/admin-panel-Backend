using AngularAdminPannel.Data;
using AngularAdminPannel.DTOs;
using AngularAdminPannel.DTOs.CMS;
using AngularAdminPannel.DTOs.Roles;
using AngularAdminPannel.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AngularAdminPannel.Services.CMSService
{
    public class CMSService : ICMSService
    {
        private readonly ApplicationDbContext _context;
        private readonly ClaimsPrincipal? _user;

        public CMSService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _user = httpContextAccessor.HttpContext?.User;
        }
        public async Task<PagedResult<CMSListItemDto>> GetCMSsAsync(CMSListFilterDto filter)
        {
            var pageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
            var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;
            var query = _context.CMSPages.AsNoTracking();
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
                    query = query.Where(u => u.Key!.Contains(searchUpper));
                }
                else if (filterType == "TITLE")
                {
                    query = query.Where(u => (u.Title ?? "").Contains(search));
                }
                else if (filterType == "METATITLE")
                {
                    query = query.Where(u => (u.MetaTitle ?? "").Contains(search));
                }
                else if (filterType == "METADESCRIPTION")
                {
                    query = query.Where(u => (u.MetaDescription ?? "").Contains(search));
                }
                else if (filterType == "METAKEYWORD")
                {
                    query = query.Where(u => (u.MetaKeyword?? "").Contains(search));
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
                else if (filterType == "ORDERTITLE")
                {
                    if (searchUpper == "ASC")
                    {
                        query = query.OrderBy(u => u.Title);
                        hasCustomOrder = true;

                    }
                    else
                    {
                        query = query.OrderByDescending(u => u.Title);
                        hasCustomOrder = true;

                    }
                }
                else if (filterType == "ORDERMETATITLE")
                {
                    if (searchUpper == "ASC")
                    {
                        query = query.OrderBy(u => u.MetaTitle);
                        hasCustomOrder = true;

                    }
                    else
                    {
                        query = query.OrderByDescending(u => u.MetaTitle);
                        hasCustomOrder = true;

                    }
                }
                else if (filterType == "ORDERMETADESCRIPTION")
                {
                    if (searchUpper == "ASC")
                    {
                        query = query.OrderBy(u => u.MetaDescription);
                        hasCustomOrder = true;

                    }
                    else
                    {
                        query = query.OrderByDescending(u => u.MetaDescription);
                        hasCustomOrder = true;

                    }
                }
                else if (filterType == "ORDERMETAKEYWORD")
                {
                    if (searchUpper == "ASC")
                    {
                        query = query.OrderBy(u => u.MetaKeyword);
                        hasCustomOrder = true;

                    }
                    else
                    {
                        query = query.OrderByDescending(u => u.MetaKeyword);
                        hasCustomOrder = true;

                    }
                }
                

            }



            //if (filter?.IsActive is bool isActive)
            //{
            //    query = query.Where(u => u.IsActive == isActive);
            //}






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

            var cmsList = await query
               
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(r => new CMSListItemDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    MetaKeyword = r.MetaKeyword,
                    MetaDescription = r.MetaDescription,
                    Key = r.Key,
                    IsActive = r.IsActive,
                    MetaTitle = r.MetaTitle,

                })
                .ToListAsync();
            return new PagedResult<CMSListItemDto>
            {
                Items = cmsList,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize

            };
        }


        public async Task<(IdentityResult Result, Guid? RoleId)> CreateAsync(CMSCreateDto model)
        {
            // Check if a CMS page with the same title already exists
            if (await _context.CMSPages.AnyAsync(r => r.Title == model.Title.Trim()))
            {
                return (IdentityResult.Failed(new IdentityError { Description = "CMS with this title already exists." }), null);
            }
            if (await _context.CMSPages.AnyAsync(r => r.Key == model.Key.Trim()))
            {
                return (IdentityResult.Failed(new IdentityError { Description = "CMS with this Key already exists." }), null);
            }
           
            var idString = _user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Safely parse string to Guid — fallback to Guid.Empty
            var _userId = Guid.TryParse(idString, out var parsedGuid) ? parsedGuid : Guid.Empty;
            var newCMS = new CMS
            {
                Id = Guid.NewGuid(), // Use Guid.NewGuid() instead of new Guid()
                Title = model.Title.Trim(),
                MetaKeyword = model.MetaKeyword,
                MetaDescription = model.MetaDescription,
                Key = model.Key,
                IsActive = model.IsActive,
                MetaTitle = model.MetaTitle,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = _userId, // Replace with actual user id if available
                ModifiedBy = _userId,
                ModifiedOn = DateTime.UtcNow,
                Content = model.Content
            };

            // Add to context
            await _context.CMSPages.AddAsync(newCMS);

            // Save changes to the database
            var result = await _context.SaveChangesAsync();

            // Check if the save was successful
            if (result > 0)
            {
                return (IdentityResult.Success, newCMS.Id);
            }

            // If save failed, return a failure result
            return (IdentityResult.Failed(new IdentityError { Description = "An error occurred while saving the CMS record." }), null);
        }


        public async Task<CMSEditDto?> GetForEditAsync(Guid id)
        {
            var cms = await _context.CMSPages.FirstOrDefaultAsync(x => x.Id == id);
            if (cms == null)
            {
                return null;

            }
            return new CMSEditDto
            {
                Id = cms.Id,
                Title = cms.Title,
                MetaKeyword = cms.MetaKeyword,
                MetaDescription = cms.MetaDescription,
                Key = cms.Key,
                IsActive = cms.IsActive,
                MetaTitle = cms.MetaTitle,
                Content=cms.Content
            };

        }


        public async Task<IdentityResult> UpdateAsync(CMSEditDto model)
        {
            // Find the existing CMS record by Id
            var cms = await _context.CMSPages.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (cms == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "NotFound",
                    Description = "CMS not found."
                });
            }

            // prevent duplicate title conflicts
            bool titleExists = await _context.CMSPages
                .AnyAsync(x => x.Id != model.Id && x.Title == model.Title.Trim());
            if (titleExists)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "DuplicateTitle",
                    Description = "Another CMS with this title already exists."
                });
            }

            bool keyExists = await _context.CMSPages
                .AnyAsync(x => x.Id != model.Id && x.Key == model.Key.Trim());
            if (keyExists)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "DuplicateTitle",
                    Description = "Another CMS with this title already exists."
                });
            }

            var idString = _user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Safely parse string to Guid — fallback to Guid.Empty
            var _userId = Guid.TryParse(idString, out var parsedGuid) ? parsedGuid : Guid.Empty;

            // Update fields
            cms.Title = model.Title.Trim();
            cms.Key = model.Key;
            cms.MetaKeyword = model.MetaKeyword;
            cms.MetaTitle = model.MetaTitle;
            cms.MetaDescription = model.MetaDescription;
            cms.Content = model.Content;
            cms.IsActive = model.IsActive;
            cms.ModifiedOn = DateTime.UtcNow;
            cms.ModifiedBy = _userId;

            // Save changes
            var result = await _context.SaveChangesAsync();

            // Return success/failure
            if (result > 0)
                return IdentityResult.Success;

            return IdentityResult.Failed(new IdentityError
            {
                Code = "UpdateFailed",
                Description = "An error occurred while updating the CMS record."
            });
        }


        public async Task<IdentityResult> ToggleActive(Guid id)
        {
            var cms = await _context.CMSPages.FirstOrDefaultAsync(x => x.Id == id);
            if (cms == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "NotFound",
                    Description = "CMS not found."
                });
            }

            cms.IsActive = !cms.IsActive;

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
                    Description = "Could not update CMS."
                });
            }
        }

        public async Task<IdentityResult> DeleteAsync(Guid id)
        {
            // Find the CMS page by Id
            var cms = await _context.CMSPages.FirstOrDefaultAsync(x => x.Id == id);
            if (cms == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "NotFound",
                    Description = "CMS not found."
                });
            }

            // Remove the record
            _context.CMSPages.Remove(cms);

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
                Description = "An error occurred while deleting the CMS record."
            });
        }


      







    }
}


    






   
