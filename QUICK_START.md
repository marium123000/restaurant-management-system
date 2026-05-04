# ⚡ QUICK START GUIDE

## 🎯 Get Your Project Running in 5 Minutes!

### Step 1: Database Setup (2 minutes)

**Option A: Automatic (Easiest)**
```bash
# Just run the app - database creates automatically!
cd OnlineRest
dotnet run
```

**Option B: Manual (If you prefer)**
```sql
-- In SQL Server Management Studio:
USE master;
DROP DATABASE IF EXISTS OnlineRestDb;
GO
-- Then run the app
```

### Step 2: Test the Application (1 minute)

1. Open browser: `https://localhost:5001`
2. You should see the homepage (if Index_NEW.cshtml is renamed)
3. Click "View Menu" to see the menu page

### Step 3: Login as Admin (1 minute)

1. Click "Login" in navigation
2. Use these credentials:
   - **Email**: `admin@restaurant.com`
   - **Password**: `Admin123@`
3. You'll be redirected to Admin Dashboard

### Step 4: Test Customer Flow (1 minute)

1. Logout
2. Click "Register"
3. Create a new account
4. You'll get 100 bonus loyalty points!
5. Browse menu and add items to cart

---

## 🔑 Default Login Credentials

### Admin Account:
```
Email: admin@restaurant.com
Password: Admin123@
```

### Customer Accounts:
```
Email: customer@gmail.com
Password: Customer123@
Loyalty Points: 150
```

```
Email: hashir@gmail.com
Password: !1@2#3
Loyalty Points: 250
```

---

## 📁 What's in the Database?

### Users (3):
- 1 Admin
- 2 Customers with loyalty points

### Food Items (15):
- **Appetizers**: Chicken Wings, Garlic Bread, Spring Rolls
- **Main Course**: Biryani, Burger, Pasta, Steak, Pizza
- **Desserts**: Lava Cake, Cheesecake, Tiramisu
- **Beverages**: Orange Juice, Iced Coffee, Smoothie, Soft Drink

### Orders (3):
- Sample orders with different statuses
- Order details with multiple items
- Payment information

---

## 🎨 View the Professional Design

### Homepage Features:
- Hero section with call-to-action
- Feature cards (Quality, Delivery, Rewards)
- Top selling items showcase
- Loyalty program promotion
- Customer testimonials

### Menu Page Features:
- Category filter tabs
- Search bar
- Food item cards with images
- Add to cart buttons
- Top selling badges
- Sales count display

---

## 🚀 Next Steps

### To Complete the Project:

1. **Rename Sample Views**:
   ```
   Index_NEW.cshtml → Index.cshtml
   Menu_NEW.cshtml → Menu.cshtml
   ```

2. **Create Remaining Views** (16 views):
   - Follow the pattern in the sample views
   - Use Bootstrap 5 components
   - Apply CSS classes from site.css
   - Add Font Awesome icons

3. **Test Everything**:
   - User registration & login
   - Browse menu & search
   - Add to cart & checkout
   - Loyalty points redemption
   - PDF receipt download
   - Table reservations
   - Admin dashboard
   - Order management

---

## 🧪 Quick Test Checklist

- [ ] Homepage loads with professional design
- [ ] Menu page shows all food items
- [ ] Category filter works
- [ ] Search functionality works
- [ ] Can register new user
- [ ] Can login as admin
- [ ] Can login as customer
- [ ] Loyalty points display in navbar
- [ ] Cart count badge shows
- [ ] No zoom issues on mobile
- [ ] Responsive on all screen sizes

---

## 🆘 Troubleshooting

### "Cannot connect to database"
```json
// Check appsettings.json:
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=OnlineRestDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### "View not found"
- Make sure you renamed Index_NEW.cshtml to Index.cshtml
- Check file is in Views/Home/ folder

### "CSS not loading"
- Press Ctrl+F5 to hard refresh
- Check _Layout.cshtml has correct CSS link

### "Database already exists error"
- Drop the database manually in SQL Server
- Or comment out `EnsureCreated()` in Program.cs

---

## 📊 Project Structure

```
OnlineRest/
├── Controllers/          ✅ 100% Complete
├── Models/              ✅ 100% Complete
├── Services/            ✅ 100% Complete
├── Views/
│   ├── Home/
│   │   ├── Index_NEW.cshtml      ✅ Sample provided
│   │   └── Menu_NEW.cshtml       ✅ Sample provided
│   ├── Account/         ⏳ To create
│   ├── Cart/            ⏳ To create
│   ├── Admin/           ⏳ To create
│   └── Shared/
│       └── _Layout.cshtml        ✅ Complete
├── wwwroot/
│   └── css/
│       └── site.css              ✅ Complete
└── Program.cs                    ✅ Complete
```

---

## 🎯 What You Have

### ✅ Complete Backend:
- All controllers with full functionality
- All models with relationships
- Database context with seed data
- PDF service for receipts
- Session management
- Authentication & authorization

### ✅ Complete Frontend Foundation:
- Professional responsive CSS
- Modern Bootstrap 5 layout
- Font Awesome icons
- Fixed viewport (no zoom!)
- 2 complete sample views

### ⏳ What You Need:
- Create 16 more views following the sample pattern
- That's it! Everything else is done!

---

## 💡 Pro Tips

1. **Copy the sample views** as templates
2. **Use Bootstrap 5 documentation** for components
3. **Use Font Awesome** for icons
4. **Test frequently** as you create views
5. **Check TempData messages** for user feedback
6. **Use validation** on all forms
7. **Keep it responsive** - test on mobile

---

## 🎉 You're Almost Done!

**Backend**: 100% ✅
**CSS & Layout**: 100% ✅
**Sample Views**: 2/18 ✅
**Remaining**: 16 views ⏳

**Estimated Time to Complete**: 4-6 hours

---

## 📞 Need Help?

Check these files:
- `README.md` - Full project documentation
- `FINAL_INSTRUCTIONS.md` - Detailed step-by-step guide
- `PROJECT_STATUS.md` - Current status and checklist
- `MIGRATION_GUIDE.md` - Database help

---

**Good luck! You've got this! 🚀**
