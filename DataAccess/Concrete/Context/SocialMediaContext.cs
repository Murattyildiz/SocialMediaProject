﻿using Core.Entities.Concrete;
using Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete.Context
{
    public class SocialMediaContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            optionsBuilder.UseSqlServer(@"Server=.;Database=SocialMediiaa;Trusted_Connection=true;TrustServerCertificate=true;");
        }

        public virtual DbSet<OperationClaim> OperationClaims { get; set; }
        public virtual DbSet<UserOperationClaim> UserOperationClaims { get; set; }
        public virtual DbSet<UserImage> UserImages { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<VerificationCode> VerificationCodes { get; set; }
        public virtual DbSet<Article> Articles { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<Topic> Topics { get; set; }

        public virtual DbSet<UserFollow> UserFollows { get; set; }
        public virtual DbSet<Message> Messages { get; set; }
        public virtual DbSet<CodeComment> CodeComments { get; set; }

        public virtual DbSet<CodeShare> CodeShares { get; set; }
    }
}