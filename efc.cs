using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCheatSheet
{
    // Ex (Table1 -> Table2 -> Table3)
    public class Table1
    {
        public int Id { get; set; }
        public int Table2Id { get; set; }
        public virtual Table2 Table2 { get; set; }
    }

    public class Table2
    {
        public int Id { get; set; }
        public int Table3Id { get; set; }
        public virtual Table3 Table3 { get; set; }
        public string Name { get; set; }
    }

    public class Table3
    {
        public int Id { get; set; }
        public string Data { get; set; }
        public decimal Value { get; set; }
    }

    /// <summary>
    /// EF Core & WinForms
    /// </summary>
    public class DbService
    {
        private readonly MyDbContext _context;

        public DbService(MyDbContext context)
        {
            _context = context;
        }

        // Table1 from ID table3.
        public Table1 GetTable1ByTable3Id(int table3Id)
        {
            return _context.Table1
                .Include(t1 => t1.Table2)
                    .ThenInclude(t2 => t2.Table3)
                .FirstOrDefault(t1 => t1.Table2.Table3.Id == table3Id);
        }

        // Table1 to table2
        public object GetAggregatedReport()
        {
            return _context.Table1
                .GroupBy(t1 => new { t1.Table2.Id, t1.Table2.Name }) 
                .Select(group => new
                {
                    Table2_Id = group.Key.Id,
                    Table2_Name = group.Key.Name,
                    CountOfTable1Records = group.Count(),           // Ex
                    AverageValue = group.Average(x => x.Table2.Table3.Value), 
                    LastData = group.Max(x => x.Table2.Table3.Data)  
                })
                .ToList(); 
        }

      /*
        var reportData = dbService.GetAggregatedReport();
        
        chart1.DataSource = reportData;
        
        
        chart1.Series[0].XValueMember = "Table2_Name"; 
        chart1.Series[0].YValueMembers = "CountOfTable1Records";
        
        chart1.DataBind();
      */

        // New Table
        public object GetCustomView()
        {
            return _context.Table1
                .Select(t => new
                {
                    MainId = t.Id,
                    Category = t.Table2.Name,
                    DetailedInfo = t.Table2.Table3.Data
                })
                .ToList();
        }

      // Filtered
      public object GetFilteredReport(string searchTerm)
      {
          var query = _context.Table1.AsQueryable();
      
          if (!string.IsNullOrWhiteSpace(searchTerm))
          {
              // Table2 searching
              query = query.Where(t => t.Table2.Name.Contains(searchTerm) || 
                                       t.Table2.Table3.Data.Contains(searchTerm));
          }
      
          return query
              .Select(t => new {
                  Name = t.Table2.Name,
                  Info = t.Table2.Table3.Data,
                  Value = t.Table2.Table3.Value
              })
              .ToList();
      }

      /* Forms:
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            // Every Update
            dataGridView1.DataSource = _dbService.GetFilteredReport(txtSearch.Text);
        }
      */
    }

    /* * (Form.cs):
     * * // Data binding to grid
     * dataGridView1.DataSource = dbService.GetAggregatedReport();
     * * // Off cols
     * if (dataGridView1.Columns["Table2_Id"] != null) 
     * dataGridView1.Columns["Table2_Id"].Visible = false;
     * * // Header editing
     * dataGridView1.Columns["Table2_Name"].HeaderText = "Name of category";
     */

    public class MyDbContext : DbContext 
    { 
        public DbSet<Table1> Table1 { get; set; }
    }
}
