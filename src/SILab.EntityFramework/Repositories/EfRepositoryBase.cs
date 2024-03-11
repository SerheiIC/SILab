using SILab.Domain.Repositories;
using SILab.Domain.Entities;
using SILab.Data;
using SILab.Collections.Extensions;
using System.Data;
using System.Data.Common;
using System.Data.Entity; 
using System.Linq.Expressions;


namespace SILab.EntityFramework.Repositories
{
    public class EfRepositoryBase<TDbContext, TEntity, TPrimaryKey> :
        RepositoryBase<TEntity, TPrimaryKey>,
        IRepositoryWithDbContext,
        ISupportsExplicitLoading<TEntity, TPrimaryKey>
        where TEntity : class, IEntity<TPrimaryKey>
        where TDbContext : DbContext
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContextProvider"></param>
        public EfRepositoryBase(IDbContextProvider<TDbContext> dbContextProvider)
        {
            _dbContextProvider = dbContextProvider;
        }

        private readonly IDbContextProvider<TDbContext> _dbContextProvider;

        /// <summary>
        /// Gets EF DbContext object.
        /// </summary>
        public virtual TDbContext DbContext => _dbContextProvider.GetDbContext();

        public DbContext GetDbContext()
        {
            return DbContext;
        }

        /// <summary>
        /// Gets DbSet for given entity.
        /// </summary>
        public virtual DbSet<TEntity> Table => DbContext.Set<TEntity>();

        public virtual DbTransaction Transaction
        {
            get
            {
                return (TransactionProvider?.GetActiveTransaction(new ActiveTransactionProviderArgs
                {
                    {"ContextType", typeof(TDbContext) }
                }) as DbTransaction) ?? throw new NullReferenceException();
            }
        }

        public virtual DbConnection Connection
        {
            get
            {
                var connection = DbContext.Database.Connection;

                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                return connection;
            }
        }

    

        public required IActiveTransactionProvider TransactionProvider { private get; set; }
      
        public override void Delete(TEntity entity)
        {
            AttachIfNot(entity);
            Table.Remove(entity);
        }
         
        public override void Delete(TPrimaryKey id)
        {
            var entity = Table.Local.FirstOrDefault(ent => EqualityComparer<TPrimaryKey>.Default.Equals(ent.Id, id))
                      ?? FirstOrDefault(id);
            if (entity == null)
            {
                return;
            }

            Delete(entity);
        }

        public void EnsureCollectionLoaded<TProperty>(TEntity entity, 
            Expression<Func<TEntity, IEnumerable<TProperty>>> collectionExpression, 
            CancellationToken cancellationToken) where TProperty : class
        {
            var expression = collectionExpression.Body as MemberExpression;
            if (expression == null)
            {
                throw new SILabException($"Given {nameof(collectionExpression)} is not a {typeof(MemberExpression).FullName}");
            }

            DbContext.Entry(entity)
                   .Collection(expression.Member.Name)
                   .Load();
        }

        public Task EnsureCollectionLoadedAsync<TProperty>(TEntity entity, 
            Expression<Func<TEntity, IEnumerable<TProperty>>> collectionExpression, 
            CancellationToken cancellationToken) where TProperty : class
        {
            var expression = collectionExpression.Body as MemberExpression
                ?? throw new SILabException($"Given {nameof(collectionExpression)} is not a {typeof(MemberExpression).FullName}");

            return DbContext.Entry(entity)
                          .Collection(expression.Member.Name)
                          .LoadAsync(cancellationToken);
        }

        public void EnsurePropertyLoaded<TProperty>(TEntity entity, 
            Expression<Func<TEntity, TProperty>> propertyExpression, 
            CancellationToken cancellationToken) where TProperty : class
        {
            DbContext.Entry(entity).Reference(propertyExpression).Load();
        }

        public Task EnsurePropertyLoadedAsync<TProperty>(TEntity entity, System.Linq.Expressions.Expression<Func<TEntity, TProperty>> propertyExpression, CancellationToken cancellationToken) where TProperty : class
        {
            return DbContext.Entry(entity)
                .Reference(propertyExpression)
                .LoadAsync(cancellationToken);
        }

        public override IQueryable<TEntity> GetAll()
        {
            return Table;
        }

        public override Task<IQueryable<TEntity>> GetAllAsync()
        {
            return Task.FromResult(Table.AsQueryable());
        }

        public override IQueryable<TEntity> GetAllIncluding(params Expression<Func<TEntity, object>>[] propertySelectors)
        {
            if (propertySelectors.IsNullOrEmpty())
            {
                return GetAll();
            }

            var query = GetAll();

            foreach (var propertySelector in propertySelectors)
            {
                query = query.Include(propertySelector);
            }

            return query;
        }
         
        public override TEntity Insert(TEntity entity)
        {
            return Table.Add(entity);
        }

        public override TEntity Update(TEntity entity)
        {
            AttachIfNot(entity);
            DbContext.Entry(entity).State = EntityState.Modified;
            return entity;
        }

        private void AttachIfNot(TEntity entity)
        {
            if (!Table.Local.Contains(entity))
            {
                Table.Attach(entity);
            }
        }
    }
}
