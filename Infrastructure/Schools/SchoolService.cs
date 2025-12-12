using Application.Features.Schools;
using Domain.Entities;
using Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Schools
{
    public class SchoolService(ApplicationDbContext context) : ISchoolService
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<int> CreateAsync(School school)
        {
            await _context.Schools.AddAsync(school);
            await _context.SaveChangesAsync();
            return school.Id;
        }

        public async Task<int> DeleteAsync(School school)
        {
            _context.Schools.Remove(school);
            await _context.SaveChangesAsync();
            return school.Id;
            
        }

        public async Task<List<School>> GetAllAsync()
        {
            return await _context.Schools.ToListAsync();
        }

        public async Task<School> GetByIdAsync(int schoolId)
        {
            return await _context.Schools.FirstOrDefaultAsync(s => s.Id == schoolId);
        }

        public async Task<School> GetByNameAsync(string schoolName)
        {
           return await _context.Schools.FirstOrDefaultAsync(s => s.Name == schoolName);
        }

        public async Task<int> UpdateAsync(School school)
        {
            _context.Schools.Update(school);
            await _context.SaveChangesAsync();
            return school.Id;
        }
    }
}
