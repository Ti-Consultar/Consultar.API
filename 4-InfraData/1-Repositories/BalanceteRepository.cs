﻿using _3_Domain._1_Entities;
using _4_InfraData._1_Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;

namespace _4_InfraData._1_Repositories
{
    public class BalanceteRepository : GenericRepository<BalanceteModel>
    {
        private readonly CoreServiceDbContext _context;
        public BalanceteRepository(CoreServiceDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<BalanceteModel>> GetByAccountPlanId(int accountPlansId)
        {
            return await _context.Balancete
                .Include(x => x.AccountPlans)
                .Where(x => x.AccountPlansId == accountPlansId)
                .ToListAsync();
        }
        public async Task<List<BalanceteModel>> GetByAccountPlanIdMonth(int accountPlansId, int year)
        {
            return await _context.Balancete
                .Include(x => x.AccountPlans)
                .Where(x => x.AccountPlansId == accountPlansId && x.DateYear == year)
                .ToListAsync();
        }
        public async Task<bool> GetExistsParams(int accountPlansId, int month, int year)
        {
            return await _context.Balancete
                .AnyAsync(x => x.AccountPlansId == accountPlansId &&
                               (int)x.DateMonth == month &&
                               x.DateYear == year);
        }

        public async Task<List<BalanceteModel>> GetById(int id)
        {
            return await _context.Balancete
                .Include(x => x.AccountPlans)
                .Where(x => x.Id == id)
                .ToListAsync();
        }
        public async Task<List<BalanceteModel>> GetBalancetesByCostCenter(int accountPlanId, int year, int bondType)
        {
            return await _context.BalanceteData
                .Where(bd => bd.CostCenter.StartsWith(bondType.ToString())
                             && bd.Balancete.DateYear == year
                             && bd.Balancete.AccountPlansId == accountPlanId)
                .Select(bd => bd.Balancete)
                .Distinct()
                .OrderBy(b => b.DateYear)
                .ThenBy(b => b.DateMonth)
                .ToListAsync();
        }

        public async Task<List<BalanceteModel>> GetByDate(int accountPlanId, int year, int month)
        {
            return await _context.Balancete
                .Include(x => x.AccountPlans)
                .Where(x => x.AccountPlansId == accountPlanId && (int)x.DateMonth == month && x.DateYear == year)
                .ToListAsync();
        }
        public async Task<List<BalanceteModel>> GetByIdDelete(int id)
        {
            return await _context.Balancete
                .Include(x => x.AccountPlans)
                .Include(x => x.BalancetesData)
                .Where(x => x.Id == id)
                .ToListAsync();
        }
        public async Task<BalanceteModel> GetBalanceteById(int id)
        {
            return await _context.Balancete
                .Include(x => x.AccountPlans)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();
        }
        public async Task<List<BalanceteModel>> GetBalancetesByCostCenters(List<string> costCenters)
        {
            return await _context.BalanceteData
                .Where(bd => costCenters.Contains(bd.CostCenter))
                .Select(bd => bd.Balancete)
                .Distinct()
                .OrderBy(b => b.DateYear)
                .ThenBy(b => b.DateMonth)
                .ToListAsync();
        }


        public async Task<List<BalanceteModel>> GetBalancetesByCostCenterAtivo(int accountPlanId, int year)
        {
            return await _context.BalanceteData
                .Where(bd => bd.CostCenter.StartsWith("1")
                             && bd.Balancete.DateYear == year
                             && bd.Balancete.AccountPlansId == accountPlanId)
                .Select(bd => bd.Balancete)
                .Distinct()
                .OrderBy(b => b.DateYear)
                .ThenBy(b => b.DateMonth)
                .ToListAsync();
        }


        public async Task DeleteBalanceteData(int balanceteId)
        {
            var model = await _context.BalanceteData
                .Where(x => x.BalanceteId == balanceteId)
                .ToListAsync();

            _context.BalanceteData.RemoveRange(model);

            await _context.SaveChangesAsync();
        }

        public async Task<List<BalanceteModel>> GetAccountPlanWithBalancetesAsync(int accountPlanId)
        {
            return await _context.Balancete
                .Include(x => x.AccountPlans)
                .Include(x => x.BalancetesData)
                .Where(ap => ap.AccountPlansId == accountPlanId)
                .OrderBy(ap => ap.DateYear)
                .ThenBy(ap => ap.DateMonth)
                .ToListAsync();
        }
        public async Task<List<BalanceteModel>> GetAccountPlanWithBalancetesMonthAsync(int accountPlanId)
        {
            return await _context.Balancete
                .Include(x => x.AccountPlans)
                .Where(ap => ap.AccountPlansId == accountPlanId)
                .OrderBy(ap => ap.DateYear)
                .ThenBy(ap => ap.DateMonth)
                .ToListAsync();
        }
        public async Task<BalanceteModel> GetWithBalancetesSearchAsync(int accountplanId, int year, int month)
        {
            return await _context.Balancete
                .Include(x => x.AccountPlans)
                .Include(x => x.BalancetesData)
                .Where(ap => ap.DateYear == year && (int)ap.DateMonth == month && ap.AccountPlansId == accountplanId)
                .OrderBy(ap => ap.DateYear)
                .ThenBy(ap => ap.DateMonth)
                .FirstOrDefaultAsync();
        }
    }
}
