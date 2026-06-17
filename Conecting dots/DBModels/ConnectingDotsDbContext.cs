using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ConnectingDotsAPI.DBModels;

public partial class ConnectingDotsDbContext : DbContext
{
    public ConnectingDotsDbContext()
    {
    }

    public ConnectingDotsDbContext(DbContextOptions<ConnectingDotsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActivityLog> ActivityLogs { get; set; }

    public virtual DbSet<ActivityLogType> ActivityLogTypes { get; set; }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<Announcement> Announcements { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<CustomerAuthToken> CustomerAuthTokens { get; set; }

    public virtual DbSet<CustomerPassword> CustomerPasswords { get; set; }

    public virtual DbSet<Download> Downloads { get; set; }

    public virtual DbSet<EmailTemplate> EmailTemplates { get; set; }

    public virtual DbSet<FollowUp> FollowUps { get; set; }

    public virtual DbSet<Form> Forms { get; set; }

    public virtual DbSet<FormResponse> FormResponses { get; set; }

    public virtual DbSet<GenericAttribute> GenericAttributes { get; set; }

    public virtual DbSet<Lead> Leads { get; set; }

    public virtual DbSet<LeadStatus> LeadStatuses { get; set; }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<Option> Options { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderNote> OrderNotes { get; set; }

    public virtual DbSet<PagesInRole> PagesInRoles { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<ProductCategoryMapping> ProductCategoryMappings { get; set; }

    public virtual DbSet<ProductType> ProductTypes { get; set; }

    public virtual DbSet<Property> Properties { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<QuestionResponse> QuestionResponses { get; set; }

    public virtual DbSet<ReferenceCode> ReferenceCodes { get; set; }

    public virtual DbSet<ReferenceType> ReferenceTypes { get; set; }

    public virtual DbSet<Setting> Settings { get; set; }

    public virtual DbSet<SitePage> SitePages { get; set; }

    public virtual DbSet<StateProvince> StateProvinces { get; set; }

    public virtual DbSet<Topic> Topics { get; set; }

    public virtual DbSet<TopicTemplate> TopicTemplates { get; set; }

    public virtual DbSet<Traceability> Traceabilities { get; set; }

    public virtual DbSet<UrlRecord> UrlRecords { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAuthToken> UserAuthTokens { get; set; }

    public virtual DbSet<UserPassword> UserPasswords { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.ToTable("ActivityLog");

            entity.Property(e => e.EntityName).HasMaxLength(400);
            entity.Property(e => e.IpAddress).HasMaxLength(200);

            entity.HasOne(d => d.ActivityLogType).WithMany(p => p.ActivityLogs)
                .HasForeignKey(d => d.ActivityLogTypeId)
                .HasConstraintName("FK_ActivityLog_ActivityLogTypeId_ActivityLogType_Id");

            entity.HasOne(d => d.User).WithMany(p => p.ActivityLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_ActivityLog_User");
        });

        modelBuilder.Entity<ActivityLogType>(entity =>
        {
            entity.ToTable("ActivityLogType");

            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.SystemKeyword).HasMaxLength(100);
        });

        modelBuilder.Entity<Address>(entity =>
        {
            entity.ToTable("Address");

            entity.HasOne(d => d.Country).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.CountryId)
                .HasConstraintName("FK_Address_CountryId_Country_Id");

            entity.HasOne(d => d.StateProvince).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.StateProvinceId)
                .HasConstraintName("FK_Address_StateProvinceId_StateProvince_Id");
        });

        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.ToTable("Announcement");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.Type).HasMaxLength(50);
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__City__3214EC07E650DEDE");

            entity.ToTable("City");

            entity.Property(e => e.DisplayOrder).HasDefaultValue(1);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Published).HasDefaultValue(true);

            entity.HasOne(d => d.Country).WithMany(p => p.Cities)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__City__CountryId__0BB1B5A5");

            entity.HasOne(d => d.State).WithMany(p => p.Cities)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__City__StateId__0CA5D9DE");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.ToTable("Country");

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.ThreeLetterIsoCode).HasMaxLength(3);
            entity.Property(e => e.TwoLetterIsoCode).HasMaxLength(2);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customer");

            entity.Property(e => e.BillingAddressId).HasColumnName("BillingAddress_Id");
            entity.Property(e => e.Email).HasMaxLength(1000);
            entity.Property(e => e.EmailToRevalidate).HasMaxLength(1000);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.ShippingAddressId).HasColumnName("ShippingAddress_Id");
            entity.Property(e => e.SystemName).HasMaxLength(400);
            entity.Property(e => e.Username).HasMaxLength(1000);

            entity.HasOne(d => d.BillingAddress).WithMany(p => p.CustomerBillingAddresses)
                .HasForeignKey(d => d.BillingAddressId)
                .HasConstraintName("FK_Customer_BillingAddress_Id_Address_Id");

            entity.HasOne(d => d.ShippingAddress).WithMany(p => p.CustomerShippingAddresses)
                .HasForeignKey(d => d.ShippingAddressId)
                .HasConstraintName("FK_Customer_ShippingAddress_Id_Address_Id");
        });

        modelBuilder.Entity<CustomerAuthToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_AuthToken");

            entity.ToTable("CustomerAuthToken");

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerAuthTokens)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentAuthToken_Student");
        });

        modelBuilder.Entity<CustomerPassword>(entity =>
        {
            entity.ToTable("CustomerPassword");

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerPasswords)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentPassword_Student");
        });

        modelBuilder.Entity<Download>(entity =>
        {
            entity.ToTable("Download");

            entity.Property(e => e.EntityName).HasMaxLength(100);
        });

        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_tbl_EmailTemplate");

            entity.ToTable("EmailTemplate");

            entity.Property(e => e.DateChanged).HasColumnType("datetime");
            entity.Property(e => e.DateCreated).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(32)
                .IsUnicode(false);
            entity.Property(e => e.SendTo)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Subject)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.SystemName)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.TemplateHtml)
                .HasColumnType("text")
                .HasColumnName("TemplateHTML");
        });

        modelBuilder.Entity<FollowUp>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__FollowUp__3214EC0791966B80");

            entity.ToTable("FollowUp");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FollowUpDate).HasColumnType("datetime");

            entity.HasOne(d => d.Lead).WithMany(p => p.FollowUps)
                .HasForeignKey(d => d.LeadId)
                .HasConstraintName("FK__FollowUp__LeadId__60C757A0");
        });

        modelBuilder.Entity<Form>(entity =>
        {
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasMany(d => d.Questions).WithMany(p => p.Forms)
                .UsingEntity<Dictionary<string, object>>(
                    "FormQuestionMapping",
                    r => r.HasOne<Question>().WithMany()
                        .HasForeignKey("QuestionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Form_Question_Mapping_Questions"),
                    l => l.HasOne<Form>().WithMany()
                        .HasForeignKey("FormId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Form_Question_Mapping_Forms"),
                    j =>
                    {
                        j.HasKey("FormId", "QuestionId");
                        j.ToTable("Form_Question_Mapping");
                    });
        });

        modelBuilder.Entity<FormResponse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__FormResp__3214EC07F5A61E02");

            entity.Property(e => e.SubmittedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Customer).WithMany(p => p.FormResponses)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FormResponses_Forms");

            entity.HasOne(d => d.Form).WithMany(p => p.FormResponses)
                .HasForeignKey(d => d.FormId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FormResponses_Forms1");
        });

        modelBuilder.Entity<GenericAttribute>(entity =>
        {
            entity.ToTable("GenericAttribute");

            entity.Property(e => e.CreatedOrUpdatedDateUtc).HasColumnName("CreatedOrUpdatedDateUTC");
            entity.Property(e => e.Key).HasMaxLength(400);
            entity.Property(e => e.KeyGroup).HasMaxLength(400);
        });

        modelBuilder.Entity<Lead>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Lead__3214EC0797DD239A");

            entity.ToTable("Lead");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.LeadName).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.AssignedToNavigation).WithMany(p => p.Leads)
                .HasForeignKey(d => d.AssignedTo)
                .HasConstraintName("FK_Lead_User");

            entity.HasOne(d => d.Customer).WithMany(p => p.Leads)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_Lead_Customer");

            entity.HasOne(d => d.LeadSource).WithMany(p => p.Leads)
                .HasForeignKey(d => d.LeadSourceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Lead_Source");

            entity.HasOne(d => d.LeadStatus).WithMany(p => p.Leads)
                .HasForeignKey(d => d.LeadStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Lead_LeadStatuses");

            entity.HasOne(d => d.Product).WithMany(p => p.Leads)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_Lead_Product");
        });

        modelBuilder.Entity<LeadStatus>(entity =>
        {
            entity.ToTable("LeadStatus");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.InputControlType).HasMaxLength(50);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK_LeadStatus_LeadStatus1");

            entity.HasOne(d => d.PreviousStatus).WithMany(p => p.InversePreviousStatus)
                .HasForeignKey(d => d.PreviousStatusId)
                .HasConstraintName("FK_LeadStatus_LeadStatus");
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.ToTable("Log");

            entity.Property(e => e.IpAddress).HasMaxLength(200);

            entity.HasOne(d => d.Customer).WithMany(p => p.Logs)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Log_CustomerId_Customer_Id");
        });

        modelBuilder.Entity<Option>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Options__3214EC079B8E65DA");

            entity.Property(e => e.Text).HasMaxLength(255);

            entity.HasOne(d => d.Question).WithMany(p => p.Options)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Options__Questio__4277DAAA");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Order");

            entity.Property(e => e.Deadline).HasColumnType("datetime");
            entity.Property(e => e.DeliveryDate).HasColumnType("datetime");
            entity.Property(e => e.Podate)
                .HasColumnType("datetime")
                .HasColumnName("PODate");

            entity.HasOne(d => d.BillingAddress).WithMany(p => p.OrderBillingAddresses)
                .HasForeignKey(d => d.BillingAddressId)
                .HasConstraintName("FK_Order_BillingAddressId_Address_Id");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_CustomerId_Customer_Id");

            entity.HasOne(d => d.PickupAddress).WithMany(p => p.OrderPickupAddresses)
                .HasForeignKey(d => d.PickupAddressId)
                .HasConstraintName("FK_Order_PickupAddressId_Address_Id");

            entity.HasOne(d => d.ShippingAddress).WithMany(p => p.OrderShippingAddresses)
                .HasForeignKey(d => d.ShippingAddressId)
                .HasConstraintName("FK_Order_ShippingAddressId_Address_Id");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItem");

            entity.Property(e => e.Color).HasMaxLength(200);
            entity.Property(e => e.Style).HasMaxLength(200);
            entity.Property(e => e.Uom)
                .HasMaxLength(200)
                .HasColumnName("UOM");
            entity.Property(e => e.Weight).HasMaxLength(200);

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_OrderItem_OrderId_Order_Id");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_OrderItem_ProductId_Product_Id");
        });

        modelBuilder.Entity<OrderNote>(entity =>
        {
            entity.ToTable("OrderNote");

            entity.Property(e => e.DisplayToCustomer).HasDefaultValue(true);

            entity.HasOne(d => d.Order).WithMany(p => p.OrderNotes)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_OrderNote_OrderId_Order_Id");
        });

        modelBuilder.Entity<PagesInRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_tbl_PagesInRoles");

            entity.Property(e => e.DateChanged).HasColumnType("datetime");
            entity.Property(e => e.DateCreated).HasColumnType("datetime");

            entity.HasOne(d => d.Page).WithMany(p => p.PagesInRoles)
                .HasForeignKey(d => d.PageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PagesInRoles_SitePage");

            entity.HasOne(d => d.Role).WithMany(p => p.PagesInRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PagesInRoles_CustomerRole");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Product");

            entity.Property(e => e.MetaKeywords).HasMaxLength(400);
            entity.Property(e => e.MetaTitle).HasMaxLength(400);
            entity.Property(e => e.Name).HasMaxLength(400);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.ProductCost).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.RedirectUrl).HasMaxLength(500);
            entity.Property(e => e.Sku).HasMaxLength(400);

            entity.HasOne(d => d.Form).WithMany(p => p.Products)
                .HasForeignKey(d => d.FormId)
                .HasConstraintName("FK_Product_Forms");

            entity.HasOne(d => d.ProductType).WithMany(p => p.Products)
                .HasForeignKey(d => d.ProductTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_ProductType");

            entity.HasMany(d => d.Forms).WithMany(p => p.ProductsNavigation)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductFormMapping",
                    r => r.HasOne<Form>().WithMany()
                        .HasForeignKey("FormId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Product_Form_Mapping_Forms"),
                    l => l.HasOne<Product>().WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Product_Form_Mapping_Product"),
                    j =>
                    {
                        j.HasKey("ProductId", "FormId");
                        j.ToTable("Product_Form_Mapping");
                    });
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.ToTable("ProductCategory");

            entity.Property(e => e.MetaKeywords).HasMaxLength(400);
            entity.Property(e => e.MetaTitle).HasMaxLength(400);
            entity.Property(e => e.Name).HasMaxLength(400);
        });

        modelBuilder.Entity<ProductCategoryMapping>(entity =>
        {
            entity.ToTable("Product_Category_Mapping");

            entity.HasOne(d => d.Category).WithMany(p => p.ProductCategoryMappings)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_Category_Mapping_ProductCategory");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductCategoryMappings)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_Category_Mapping_Product");
        });

        modelBuilder.Entity<ProductType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_NewTable");

            entity.ToTable("ProductType");

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.SystemKeyword).HasMaxLength(255);
        });

        modelBuilder.Entity<Property>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Property__70C9A755435DB836");

            entity.ToTable("Property");

            entity.Property(e => e.AddressLine1).HasMaxLength(255);
            entity.Property(e => e.AddressLine2).HasMaxLength(255);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ListingDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PropertyName).HasMaxLength(255);
            entity.Property(e => e.PropertyType).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CountryNavigation).WithMany(p => p.Properties)
                .HasForeignKey(d => d.Country)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Property_Country");

            entity.HasOne(d => d.StateNavigation).WithMany(p => p.Properties)
                .HasForeignKey(d => d.State)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Property_StateProvince");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Question__3214EC07A9DD6067");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.ControlType).HasMaxLength(50);
            entity.Property(e => e.Text).HasMaxLength(255);
        });

        modelBuilder.Entity<QuestionResponse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Question__3214EC076EA8D706");

            entity.HasOne(d => d.FormResponse).WithMany(p => p.QuestionResponses)
                .HasForeignKey(d => d.FormResponseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_QuestionResponses_QuestionResponses");

            entity.HasOne(d => d.Question).WithMany(p => p.QuestionResponses)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_QuestionResponses_Questions");
        });

        modelBuilder.Entity<ReferenceCode>(entity =>
        {
            entity.ToTable("ReferenceCode");

            entity.Property(e => e.Name).HasMaxLength(1000);
            entity.Property(e => e.SortOrder).HasDefaultValue(1);
            entity.Property(e => e.SystemKeyword).HasMaxLength(1000);

            entity.HasOne(d => d.ReferenceCodeNavigation).WithMany(p => p.InverseReferenceCodeNavigation)
                .HasForeignKey(d => d.ReferenceCodeId)
                .HasConstraintName("FK_ReferenceCode_ReferenceCode");

            entity.HasOne(d => d.ReferenceType).WithMany(p => p.ReferenceCodes)
                .HasForeignKey(d => d.ReferenceTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReferenceCode_ReferenceType");
        });

        modelBuilder.Entity<ReferenceType>(entity =>
        {
            entity.ToTable("ReferenceType");

            entity.Property(e => e.Enabled).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(1000);
            entity.Property(e => e.SystemKeyword).HasMaxLength(1000);
        });

        modelBuilder.Entity<Setting>(entity =>
        {
            entity.ToTable("Setting");

            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<SitePage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_tbl_SitePage");

            entity.ToTable("SitePage");

            entity.Property(e => e.DateChanged).HasColumnType("datetime");
            entity.Property(e => e.DateCreated).HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.SystemName).HasMaxLength(50);
        });

        modelBuilder.Entity<StateProvince>(entity =>
        {
            entity.ToTable("StateProvince");

            entity.Property(e => e.Abbreviation).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Country).WithMany(p => p.StateProvinces)
                .HasForeignKey(d => d.CountryId)
                .HasConstraintName("FK_StateProvince_CountryId_Country_Id");
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.ToTable("Topic");
        });

        modelBuilder.Entity<TopicTemplate>(entity =>
        {
            entity.ToTable("TopicTemplate");

            entity.Property(e => e.Name).HasMaxLength(400);
            entity.Property(e => e.ViewPath).HasMaxLength(400);
        });

        modelBuilder.Entity<Traceability>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Tracibility");

            entity.ToTable("Traceability");

            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");

            entity.HasOne(d => d.ManagerUser).WithMany(p => p.TraceabilityManagerUsers)
                .HasForeignKey(d => d.ManagerUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tracibility_User");

            entity.HasOne(d => d.ParentNode).WithMany(p => p.InverseParentNode)
                .HasForeignKey(d => d.ParentNodeId)
                .HasConstraintName("FK_Tracibility_Tracibility");

            entity.HasOne(d => d.Product).WithMany(p => p.Traceabilities)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_Tracibility_Product");

            entity.HasOne(d => d.Supplier).WithMany(p => p.TraceabilitySuppliers)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK_Tracibility_User1");

            entity.HasOne(d => d.Type).WithMany(p => p.Traceabilities)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tracibility_ReferenceCode");
        });

        modelBuilder.Entity<UrlRecord>(entity =>
        {
            entity.ToTable("UrlRecord");

            entity.Property(e => e.EntityName).HasMaxLength(400);
            entity.Property(e => e.Slug).HasMaxLength(400);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.FirstName).HasMaxLength(200);
            entity.Property(e => e.LastName).HasMaxLength(200);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRoleMapping",
                    r => r.HasOne<UserRole>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_UserRole_Mapping_UserRole"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_UserRole_Mapping_User"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId").HasName("PK_Customer_CustomerRole_Mapping");
                        j.ToTable("UserRole_Mapping");
                        j.IndexerProperty<int>("UserId").HasColumnName("User_Id");
                        j.IndexerProperty<int>("RoleId").HasColumnName("Role_Id");
                    });
        });

        modelBuilder.Entity<UserAuthToken>(entity =>
        {
            entity.ToTable("UserAuthToken");

            entity.HasOne(d => d.User).WithMany(p => p.UserAuthTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAuthToken_User");
        });

        modelBuilder.Entity<UserPassword>(entity =>
        {
            entity.ToTable("UserPassword");

            entity.HasOne(d => d.User).WithMany(p => p.UserPasswords)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserPassword_User");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_CustomerRole");

            entity.ToTable("UserRole");

            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.SystemName).HasMaxLength(255);

            entity.HasMany(d => d.ParentRoles).WithMany(p => p.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "RoleAssignment",
                    r => r.HasOne<UserRole>().WithMany()
                        .HasForeignKey("ParentRoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_RoleAssignment_UserRole_Parent"),
                    l => l.HasOne<UserRole>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_RoleAssignment_UserRole"),
                    j =>
                    {
                        j.HasKey("RoleId", "ParentRoleId");
                        j.ToTable("RoleAssignment");
                    });

            entity.HasMany(d => d.Roles).WithMany(p => p.ParentRoles)
                .UsingEntity<Dictionary<string, object>>(
                    "RoleAssignment",
                    r => r.HasOne<UserRole>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_RoleAssignment_UserRole"),
                    l => l.HasOne<UserRole>().WithMany()
                        .HasForeignKey("ParentRoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_RoleAssignment_UserRole_Parent"),
                    j =>
                    {
                        j.HasKey("RoleId", "ParentRoleId");
                        j.ToTable("RoleAssignment");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
