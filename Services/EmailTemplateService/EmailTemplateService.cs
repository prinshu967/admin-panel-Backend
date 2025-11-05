using AngularAdminPanel.DTOs.EmailTemplate;
using AngularAdminPannel.Data;
using AngularAdminPannel.DTOs;
using AngularAdminPannel.DTOs.CMS;
using AngularAdminPannel.DTOs.EmailTemplate;
using AngularAdminPannel.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace AngularAdminPannel.Services.EmailTemplateService
{
    public class EmailTemplateService:IEmailTemplateService
    {
        private readonly ApplicationDbContext _context;
        private readonly ClaimsPrincipal? _user;
        public EmailTemplateService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _user = httpContextAccessor.HttpContext?.User;
        }
        public async Task<PagedResult<EmailTemplateListItemDto>> GetEmailTemplatesAsync(EmailTemplateListFilterDto filter)
        {
            var pageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
            var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;
            var query = _context.EmailTemplates.AsNoTracking();
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
                    hasCustomOrder = true;
                }
                else if (filterType == "TITLE")
                {
                    query = query.Where(u => (u.Title ?? "").Contains(search));
                    hasCustomOrder = true;
                }
                else if (filterType == "SUBJECT")
                {
                    query = query.Where(u => (u.Subject ?? "").Contains(search));
                    hasCustomOrder = true;

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
                else if (filterType == "ORDERSUBJECT")
                {
                    if (searchUpper == "ASC")
                    {
                        query = query.OrderBy(u => u.Subject);
                        hasCustomOrder = true;

                    }
                    else
                    {
                        query = query.OrderByDescending(u => u.Subject);
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

            var emailTemplates = await query
               
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(r => new EmailTemplateListItemDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    Key = r.Key,
                    IsActive = r.IsActive,
                    Subject = r.Subject,
                   

                })
                .ToListAsync();
            return new PagedResult<EmailTemplateListItemDto>
            {
                Items = emailTemplates,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize

            };
        }


        public async Task<(IdentityResult Result, Guid? RoleId)> CreateAsync(EmailTemplateCreateDto model)
        {
            // Trim common inputs for consistency
            var title = model.Title.Trim();
            var key = model.Key.Trim();

            // Check if an email template with the same Title already exists
            if (await _context.EmailTemplates.AnyAsync(r => r.Title == title))
            {
                return (IdentityResult.Failed(new IdentityError
                {
                    Description = "An email template with this title already exists."
                }), null);
            }

            // Check if an email template with the same Key already exists
            if (await _context.EmailTemplates.AnyAsync(r => r.Key == key))
            {
                return (IdentityResult.Failed(new IdentityError
                {
                    Description = "An email template with this key already exists."
                }), null);
            }


            var idString = _user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Safely parse string to Guid — fallback to Guid.Empty
            var _userId = Guid.TryParse(idString, out var parsedGuid) ? parsedGuid : Guid.Empty;

            // Create a new EmailTemplate record
            var emailTemplate = new EmailTemplate
            {
                Id = Guid.NewGuid(), 
                Key = key,
                Title = title,
                Subject = model.Subject.Trim(),
                FromName = model.FromName.Trim(),
                FromEmail = model.FromEmail.Trim(),
                IsActive = model.IsActive,
                IsManualMail = model.IsManualMail,
                IsContactUsMail = model.IsContactUsMail,
                Body = model.Body, 

                CreatedOn = DateTime.UtcNow,
                CreatedBy = _userId, 
                ModifiedOn = DateTime.UtcNow,
                ModifiedBy = _userId
            };

            // Add to context
            await _context.EmailTemplates.AddAsync(emailTemplate);

            // Save changes to the database
            var saveResult = await _context.SaveChangesAsync();

            // Return success or failure
            if (saveResult > 0)
                return (IdentityResult.Success, emailTemplate.Id);

            return (IdentityResult.Failed(new IdentityError
            {
                Description = "An error occurred while saving the email template."
            }), null);
        }


        public async Task<EmailTemplateEditDto?> GetForEditAsync(Guid id)
        {
            var template= await _context.EmailTemplates.FirstOrDefaultAsync(x => x.Id == id);
            if (template == null)
            {
                return null;
            }

            return new EmailTemplateEditDto
            {
                Id = template.Id,
                Key = template.Key,
                Title = template.Title,
                Subject = template.Subject,
                FromName = template.FromName,
                FromEmail = template.FromEmail,
                IsActive = template.IsActive,
                IsManualMail = template.IsManualMail,
                IsContactUsMail = template.IsContactUsMail,
                Body = template.Body
            };
        }

        

        public async Task<IdentityResult> UpdateAsync(EmailTemplateEditDto model)
        {
            // Find the existing email template by Id
            var emailTemplate = await _context.EmailTemplates.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (emailTemplate == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "NotFound",
                    Description = "Email template not found."
                });
            }

            // Prevent duplicate title conflicts
            bool titleExists = await _context.EmailTemplates
                .AnyAsync(x => x.Id != model.Id && x.Title.Trim() == model.Title.Trim());
            if (titleExists)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "DuplicateTitle",
                    Description = "Another email template with this title already exists."
                });
            }

            // Prevent duplicate key conflicts
            bool keyExists = await _context.EmailTemplates
                .AnyAsync(x => x.Id != model.Id && x.Key.Trim() == model.Key.Trim());
            if (keyExists)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "DuplicateKey",
                    Description = "Another email template with this key already exists."
                });
            }
            var idString = _user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Safely parse string to Guid — fallback to Guid.Empty
            var _userId = Guid.TryParse(idString, out var parsedGuid) ? parsedGuid : Guid.Empty;

            // Update fields
            emailTemplate.Key = model.Key.Trim();
            emailTemplate.Title = model.Title.Trim();
            emailTemplate.Subject = model.Subject.Trim();
            emailTemplate.FromName = model.FromName.Trim();
            emailTemplate.FromEmail = model.FromEmail.Trim();
            emailTemplate.Body = model.Body;
            emailTemplate.IsActive = model.IsActive;
            emailTemplate.IsManualMail = model.IsManualMail;
            emailTemplate.IsContactUsMail = model.IsContactUsMail;
            emailTemplate.ModifiedOn = DateTime.UtcNow;
            emailTemplate.ModifiedBy = _userId; 
            


            // Save changes
            var result = await _context.SaveChangesAsync();

            // Return success/failure
            if (result > 0)
                return IdentityResult.Success;

            return IdentityResult.Failed(new IdentityError
            {
                Code = "UpdateFailed",
                Description = "An error occurred while updating the email template."
            });
        }

        public async Task<IdentityResult> ToggleActive(Guid id)
        {
            var template = await _context.EmailTemplates.FirstOrDefaultAsync(x => x.Id == id);
            if (template == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "NotFound",
                    Description = "Email Template not found."
                });
            }

            template.IsActive = !template.IsActive;

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
                    Description = "Could not update Email Template."
                });
            }
        }

        public async Task<IdentityResult> DeleteAsync(Guid id)
        {
            // Find the CMS page by Id
            var emailTemplate = await _context.EmailTemplates.FirstOrDefaultAsync(x => x.Id == id);
            if (emailTemplate == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "NotFound",
                    Description = "EmailTemplate not found."
                });
            }

            // Remove the record
            _context.EmailTemplates.Remove(emailTemplate);

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
                Description = "An error occurred while deleting the Email Template record."
            });
        }


    }
}
