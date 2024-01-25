using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using KatlaSport.DataAccess;
using KatlaSport.DataAccess.ProductStoreHive;
using DbHiveSection = KatlaSport.DataAccess.ProductStoreHive.StoreHiveSection;

namespace KatlaSport.Services.HiveManagement
{
    /// <summary>
    /// Represents a hive section service.
    /// </summary>
    public class HiveSectionService : IHiveSectionService
    {
        private readonly IProductStoreHiveContext _context;
        private readonly IUserContext _userContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="HiveSectionService"/> class with specified <see cref="IProductStoreHiveContext"/> and <see cref="IUserContext"/>.
        /// </summary>
        /// <param name="context">A <see cref="IProductStoreHiveContext"/>.</param>
        /// <param name="userContext">A <see cref="IUserContext"/>.</param>
        public HiveSectionService(IProductStoreHiveContext context, IUserContext userContext)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userContext = userContext ?? throw new ArgumentNullException();
        }

        /// <inheritdoc/>
        public async Task<List<HiveSectionListItem>> GetHiveSectionsAsync()
        {
            var dbHiveSections = await _context.Sections.OrderBy(s => s.Id).ToArrayAsync();
            var hiveSections = dbHiveSections.Select(s => Mapper.Map<HiveSectionListItem>(s)).ToList();
            return hiveSections;
        }

        /// <inheritdoc/>
        public async Task<HiveSection> GetHiveSectionAsync(int hiveSectionId)
        {
            var dbHiveSections = await _context.Sections.Where(s => hiveSectionId == s.Id).ToArrayAsync();
            if (dbHiveSections.Length == 0)
            {
                throw new RequestedResourceNotFoundException();
            }

            return Mapper.Map<DbHiveSection, HiveSection>(dbHiveSections[0]);
        }

        /// <inheritdoc/>
        public async Task<HiveSection> CreateHiveSectionAsync(int hiveId, UpdateHiveSectionRequest createRequest)
        {
            var dbSections = await _context.Sections.Where(h => h.Code == createRequest.Code).ToArrayAsync();
            if (dbSections.Length > 0)
            {
                throw new RequestedResourceHasConflictException("code");
            }

            var dbSection = new DbHiveSection
            {
                Code = createRequest.Code,
                Name = createRequest.Name,
                StoreHiveId = hiveId,
                Created = DateTime.Now,
                LastUpdated = DateTime.Now,
                CreatedBy = _userContext.UserId,
                LastUpdatedBy = _userContext.UserId
            };

            _context.Sections.Add(dbSection);
            await _context.SaveChangesAsync();

            return Mapper.Map<HiveSection>(dbSection);
        }

        /// <inheritdoc/>
        public async Task<HiveSection> UpdateHiveSectionAsync(int hiveSectionId, UpdateHiveSectionRequest updateRequest)
        {
            var dbSections = await _context.Sections.Where(p => p.Code == updateRequest.Code && p.Id != hiveSectionId).ToArrayAsync();
            if (dbSections.Length > 0)
            {
                throw new RequestedResourceHasConflictException("code");
            }

            dbSections = await _context.Sections.Where(p => p.Id == hiveSectionId).ToArrayAsync();
            if (dbSections.Length == 0)
            {
                throw new RequestedResourceNotFoundException();
            }

            var dbHive = dbSections[0];

            Mapper.Map(updateRequest, dbHive);
            dbHive.LastUpdatedBy = _userContext.UserId;

            await _context.SaveChangesAsync();

            return Mapper.Map<HiveSection>(dbHive);
        }

        /// <inheritdoc/>
        public async Task DeleteHiveSectionAsync(int hiveSectionId)
        {
            var dbSections = await _context.Sections.Where(p => p.Id == hiveSectionId).ToArrayAsync();
            if (dbSections.Length == 0)
            {
                throw new RequestedResourceNotFoundException();
            }

            var dbHive = dbSections[0];
            if (dbHive.IsDeleted == false)
            {
                throw new RequestedResourceHasConflictException();
            }

            _context.Sections.Remove(dbHive);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<List<HiveSectionListItem>> GetHiveSectionsAsync(int hiveId)
        {
            var dbHiveSections = await _context.Sections.Where(s => s.StoreHiveId == hiveId).OrderBy(s => s.Id).ToArrayAsync();
            var hiveSections = dbHiveSections.Select(s => Mapper.Map<HiveSectionListItem>(s)).ToList();
            return hiveSections;
        }

        /// <inheritdoc/>
        public async Task SetStatusAsync(int hiveSectionId, bool deletedStatus)
        {
            var dbSections = await _context.Sections.Where(c => c.Id == hiveSectionId).ToArrayAsync();

            if (dbSections.Length == 0)
            {
                throw new RequestedResourceNotFoundException();
            }

            var dbCategory = dbSections[0];
            if (dbCategory.IsDeleted != deletedStatus)
            {
                dbCategory.IsDeleted = deletedStatus;
                dbCategory.LastUpdated = DateTime.UtcNow;
                dbCategory.LastUpdatedBy = _userContext.UserId;
                await _context.SaveChangesAsync();
            }
        }
    }
}
