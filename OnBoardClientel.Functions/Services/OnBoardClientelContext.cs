using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnBoardClientel.Functions.Services
{
    public class OnBoardClientelContext: DbContext
    {
        public OnBoardClientelContext(DbContextOptions options):base(options)
        {

        }
        public DbSet<Client> Clients { get; set; }
    }
}
