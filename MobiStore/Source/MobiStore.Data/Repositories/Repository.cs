﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

using MobiStore.Data.Contracts;

namespace MobiStore.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private ISqlServerContext context;
        private IDbSet<T> set;

        public Repository(ISqlServerContext context)
        {
            this.context = context;
            this.set = context.Set<T>();
        }
        
        public IQueryable<T> All()
        {
            return this.set.AsQueryable();
        }

        public IQueryable<T> Find(Expression<Func<T, bool>> conditions)
        {
            return this.All().Where(conditions);
        }

        public void Add(T entity)
        {
            var entry = this.AttachIfDetached(entity);
            entry.State = EntityState.Added;
        }

        public void Update(T entity)
        {
            var entry = this.AttachIfDetached(entity);
            entry.State = EntityState.Modified;
        }

        public void Delete(T entity)
        {
            var entry = this.AttachIfDetached(entity);
            entry.State = EntityState.Deleted;
        }

        public void Detach(T entity)
        {
            var entry = this.context.Entry(entity);
            entry.State = EntityState.Detached;
        }

        private DbEntityEntry AttachIfDetached(T entity)
        {
            var entry = this.context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                this.set.Attach(entity);
            }

            return entry;
        }
    }
}