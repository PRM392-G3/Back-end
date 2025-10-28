using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using SocialNetworkMobile.Repository;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SocialNetworkMobile.Repository.Models;
using SocialNetworkMobile.Repository.Context;

namespace SocialNetworkMobile.Repository.Basic
{
    public class GenericRepository<T>  where T : class
    {
        protected SocialNetworkDbContext _context;

        public GenericRepository()
        {
            _context ??= new SocialNetworkDbContext();
        }

        public GenericRepository(SocialNetworkDbContext context)
        {
            _context = context;
        }
        public async Task<TKey> CreateReturnKeyAsync<TKey>(T entity)
        {
            try
            {
                // Check entity null
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                // Add entity to DbSet
                await _context.Set<T>().AddAsync(entity);

                // Save changes to database
                await _context.SaveChangesAsync();

                // Get primary key metadata
                var keyProperty = _context.Entry(entity).Metadata.FindPrimaryKey()?.Properties[0];
                if (keyProperty == null)
                    throw new InvalidOperationException($"Entity {typeof(T).Name} does not have a primary key defined.");

                // Get primary key value
                var keyValue = _context.Entry(entity).Property(keyProperty.Name).CurrentValue;

                // Cast to TKey
                return (TKey)Convert.ChangeType(keyValue, typeof(TKey));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating {typeof(T).Name}: {ex.Message}", ex);
            }
        }
        public List<T> GetAll()
        {
            return _context.Set<T>().ToList();
        }
        public async Task<List<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }
        public void Create(T entity)
        {
            _context.Add(entity);
            _context.SaveChanges();
        }


        public async Task<int> CreateAsync(T entity)
        {
            try
            {
                // Ensure all DateTime properties are UTC
                EnsureDateTimeUtc(entity);
                
                _context.Add(entity);
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating {typeof(T).Name}: {ex.Message}", ex);
            }
        }
        /// <summary>
        /// Lấy IQueryable để xây dựng truy vấn phức tạp (VỚI tracking)
        /// </summary>
        public IQueryable<T> GetQueryableWithTracking()
        {
            return _context.Set<T>();
        }

        /// <summary>
        /// Lấy IQueryable để xây dựng truy vấn phức tạp (KHÔNG tracking)
        /// Tối ưu cho các truy vấn CHỈ ĐỌC (READ-ONLY)
        /// </summary>
        public IQueryable<T> GetQueryable()
        {
            return _context.Set<T>().AsNoTracking();
        }

        /// <summary>
        /// Lấy IQueryable (KHÔNG tracking) VỚI điều kiện
        /// </summary>
        public IQueryable<T> GetQueryable(Expression<Func<T, bool>> predicate)
        {
            return _context.Set<T>().Where(predicate).AsNoTracking();
        }

        public async Task<int> CreateAsyncWithCheckExist(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            try
            {
                // Ensure all DateTime properties are UTC
                EnsureDateTimeUtc(entity);

                // Get the primary key property
                var keyProperty = _context.Entry(entity).Metadata.FindPrimaryKey()?.Properties[0];
                if (keyProperty == null)
                    throw new InvalidOperationException($"Entity {typeof(T).Name} does not have a primary key defined.");

                // Get the key value
                var keyValue = _context.Entry(entity).Property(keyProperty.Name).CurrentValue;

                // Check if entity already exists
                var existingEntity = await _context.Set<T>().FindAsync(keyValue);
                if (existingEntity != null)
                    throw new InvalidOperationException($"Entity {typeof(T).Name} with key {keyValue} already exists.");

                await _context.Set<T>().AddAsync(entity);
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating {typeof(T).Name}: {ex.Message}", ex);
            }
        }


        public void Update(T entity)
        {
            //// Turning off Tracking for UpdateAsync in Entity Framework
            _context.ChangeTracker.Clear();
            var tracker = _context.Attach(entity);
            tracker.State = EntityState.Modified;
            _context.SaveChanges();
        }

        public async Task<int> UpdateAsync(T entity)
        {
            try
            {
                Console.WriteLine($"UpdateAsync called for {typeof(T).Name}");
                
                // Ensure all DateTime properties are UTC
                EnsureDateTimeUtc(entity);
                
                //// Turning off Tracking for UpdateAsync in Entity Framework
                _context.ChangeTracker.Clear();
                var tracker = _context.Attach(entity);
                tracker.State = EntityState.Modified;
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateAsync: {ex.Message}");
                throw new Exception($"Error updating {typeof(T).Name}: {ex.Message}", ex);
            }
        }

        public bool Remove(T entity)
        {
            _context.Remove(entity);
            _context.SaveChanges();
            return true;
        }

        public async Task<bool> RemoveAsync(T entity)
        {
            _context.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public T GetById(int id)
        {
            return _context.Set<T>().Find(id);
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public T GetById(string code)
        {
            return _context.Set<T>().Find(code);
        }

        public async Task<T> GetByIdAsync(string code)
        {
            return await _context.Set<T>().FindAsync(code);
        }

        /*
        https://guidgenerator.com/
        daacb4fb-ff73-46ef-98f1-4af9aab2a30a
         */
        public T GetById(Guid code)
        {
            return _context.Set<T>().Find(code);
        }

        public async Task<T> GetByIdAsync(Guid code)
        {
            return await _context.Set<T>().FindAsync(code);
        }

        #region Separating asigned entity and save operators        

        public void PrepareCreate(T entity)
        {
            _context.Add(entity);
        }

        public void PrepareUpdate(T entity)
        {
            var tracker = _context.Attach(entity);
            tracker.State = EntityState.Modified;
        }

        public void PrepareRemove(T entity)
        {
            _context.Remove(entity);
        }

        public int Save()
        {
            return _context.SaveChanges();
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        #endregion Separating asign entity and save operators

        // Additional methods required by interface
        public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().Where(predicate).ToListAsync();
        }

        public async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(predicate);
        }

        public async Task DeleteAsync(T entity)
        {
            _context.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().CountAsync(predicate);
        }

        public async Task<int> CountAsync()
        {
            return await _context.Set<T>().CountAsync();
        }

        /// <summary>
        /// Ensures all DateTime properties in the entity are UTC
        /// </summary>
        /// <param name="entity">The entity to process</param>
        private void EnsureDateTimeUtc(T entity)
        {
            if (entity == null) return;

            var properties = typeof(T).GetProperties()
                .Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?));

            foreach (var property in properties)
            {
                var value = property.GetValue(entity);
                if (value is DateTime dateTime)
                {
                    Console.WriteLine($"DateTime property {property.Name}: {dateTime} (Kind: {dateTime.Kind})");
                    
                    // Convert to UTC if not already UTC
                    if (dateTime.Kind != DateTimeKind.Utc)
                    {
                        var utcDateTime = dateTime.Kind == DateTimeKind.Local 
                            ? dateTime.ToUniversalTime() 
                            : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                        property.SetValue(entity, utcDateTime);
                        Console.WriteLine($"Converted {property.Name} to UTC: {utcDateTime} (Kind: {utcDateTime.Kind})");
                    }
                }
                else if (value is DateTime nullableDateTime)
                {
                    Console.WriteLine($"Nullable DateTime property {property.Name}: {nullableDateTime} (Kind: {nullableDateTime.Kind})");
                    
                    if (nullableDateTime.Kind != DateTimeKind.Utc)
                    {
                        var utcDateTime = nullableDateTime.Kind == DateTimeKind.Local 
                            ? nullableDateTime.ToUniversalTime() 
                            : DateTime.SpecifyKind(nullableDateTime, DateTimeKind.Utc);
                        property.SetValue(entity, utcDateTime);
                        Console.WriteLine($"Converted nullable {property.Name} to UTC: {utcDateTime} (Kind: {utcDateTime.Kind})");
                    }
                }
            }
        }
    }
}
