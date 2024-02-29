﻿using Microsoft.EntityFrameworkCore;
using MsBanking.Core.Account.Domain.Const;

namespace MsBanking.Core.Account.Domain
{
    public class DataSeeder
    {
        public static void Seed(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AccountDbContext>();

                if (context == null)
                    return;
                
                context.Database.Migrate();
                context.Database.EnsureCreated();

                if (!context.Accounts.Any())
                {
                    var accountList = new List<MsBanking.Common.Entity.Account>()
                    {
                        new Common.Entity.Account() { AccountNumber = "1234567890", IbanNumber = "TR1234567890", AccountType = (int)AccountTypeEnum.Retail, Balance = 1000, Currency = "TRY", UserId = "1234567890", BranchId = 9019, AccountSuffix = 1,IsActive=true },
                        new Common.Entity.Account() { AccountNumber = "1234567891", IbanNumber = "TR1234567891", AccountType = (int)AccountTypeEnum.Retail, Balance = 1000, Currency = "TRY", UserId = "1234567891", BranchId = 9142, AccountSuffix = 1,IsActive = true },
                        new Common.Entity.Account() { AccountNumber = "1234567892", IbanNumber = "TR1234567892", AccountType = (int)AccountTypeEnum.Retail, Balance = 1000, Currency = "TRY", UserId = "1234567892", BranchId = 9080, AccountSuffix = 1,IsActive=true },
                    };

                    context.AddRange(accountList);
                    context.SaveChanges();
                }
            }
        }   
    }
}
