# Quick Test Guide - Admin-Customer Separation

## 🚀 How to Test the Changes

### Step 1: Start the Application
```bash
cd OnlineRest
dotnet run
```
Application will start at: **http://localhost:5179**

---

## 🔐 Test Accounts

### Admin Account
- **Email**: admin@restaurant.com
- **Password**: admin123

### Customer Accounts
- **Email**: john@example.com | **Password**: password123
- **Email**: jane@example.com | **Password**: password123

---

## ✅ Admin User Testing (5 minutes)

### 1. Login as Admin
- Go to http://localhost:5179/Account/Login
- Login with admin credentials
- ✅ Should redirect to Admin Dashboard

### 2. Check Navigation
- ✅ Should see: Dashboard, Food Items, Orders, Reservations, Sales Report
- ❌ Should NOT see: Cart icon, Loyalty points, Profile link, Reservations (customer)

### 3. Try Accessing Customer Features (Should All Fail)
Try these URLs manually in browser:

| URL | Expected Result |
|-----|----------------|
| `/Cart/Index` | Redirect to dashboard with error |
| `/Home/Reservations` | Redirect to dashboard with error |
| `/Account/Profile` | Redirect to dashboard with error |
| `/Home/OrderHistory` | Redirect to dashboard with error |

### 4. Verify Admin Features Work
- ✅ Click "Food Items" → Should work
- ✅ Click "Orders" → Should work
- ✅ Click "Reservations" → Should work (admin view)
- ✅ Click "Sales Report" → Should work

---

## ✅ Customer User Testing (5 minutes)

### 1. Logout and Login as Customer
- Logout from admin
- Login with john@example.com / password123
- ✅ Should redirect to Home page

### 2. Check Navigation
- ✅ Should see: Menu, Reservations, Cart, Orders, Profile
- ✅ Should see: Loyalty points badge (100 points)
- ✅ Should see: Cart icon with count

### 3. Test Customer Features (Should All Work)

#### Test Cart
1. Click "Menu"
2. Click "Add to Cart" on any item
3. ✅ Should see success message
4. ✅ Cart count should increase
5. Click cart icon
6. ✅ Should see cart page with items

#### Test Reservations
1. Click "Reservations"
2. ✅ Should see reservation form
3. Fill out form and submit
4. ✅ Should see success message

#### Test Profile
1. Click username dropdown → "Profile"
2. ✅ Should see profile page with loyalty points
3. Try updating name
4. ✅ Should update successfully

#### Test Orders
1. Click "Orders"
2. ✅ Should see order history
3. Click on an order
4. ✅ Should see order details

---

## 🎯 Quick Verification Checklist

### Admin Cannot Access:
- [ ] Cart (Index, Add, Update, Remove, Clear)
- [ ] Checkout (GET and POST)
- [ ] Customer Reservations (View, Create, Cancel)
- [ ] Customer Order History
- [ ] Customer Profile
- [ ] Order Confirmation
- [ ] Download Customer Receipts

### Customer Can Access:
- [ ] Full cart functionality
- [ ] Checkout and place orders
- [ ] Make and cancel reservations
- [ ] View and edit profile
- [ ] View order history
- [ ] Download receipts
- [ ] Earn and use loyalty points

### Admin Can Access:
- [ ] Admin Dashboard
- [ ] Manage Food Items (Create, Edit, Delete)
- [ ] View All Orders (all customers)
- [ ] View All Reservations (all customers)
- [ ] Generate Sales Reports

---

## 🐛 What to Look For

### Success Indicators:
✅ Admin redirected to dashboard when accessing customer features
✅ Error messages display in red at top of page
✅ Customer features work normally for customer accounts
✅ No console errors or exceptions
✅ Navigation shows/hides appropriate links

### Potential Issues:
❌ Admin can access cart or reservations
❌ Customer cannot access their features
❌ Error messages don't display
❌ Redirects don't work
❌ Application crashes or shows errors

---

## 📊 Expected Error Messages

When admin tries to access customer features:

| Feature | Error Message |
|---------|--------------|
| Cart | "Admin users cannot use the shopping cart." |
| Add to Cart | "Admin users cannot add items to cart." |
| Checkout | "Admin users cannot checkout." |
| Reservations | "Admin users cannot make reservations. Please use a customer account." |
| Profile | "Admin users don't have customer profiles." |
| Order History | "Admin users should use the Admin Dashboard to view orders." |

---

## 🔧 Troubleshooting

### If Application Won't Start:
```bash
# Check if port is in use
netstat -ano | findstr :5179

# Kill process if needed (replace PID)
taskkill /PID <PID> /F

# Try again
dotnet run
```

### If Database Errors Occur:
The database should already be set up. If you see schema errors:
```bash
# Drop and recreate database
sqlcmd -S ".\SQLEXPRESS" -C -Q "USE master; ALTER DATABASE OnlineRestDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE OnlineRestDb;"

# Restart application (will recreate database)
dotnet run
```

### If Session Issues Occur:
- Clear browser cookies
- Use incognito/private browsing
- Restart the application

---

## ⏱️ Total Testing Time: ~10 minutes

1. **Admin Testing**: 5 minutes
2. **Customer Testing**: 5 minutes

---

## 📝 Report Issues

If you find any issues, note:
1. Which user type (admin/customer)
2. What action you tried
3. Expected vs actual result
4. Any error messages
5. Browser console errors (F12)

---

## ✨ Success Criteria

All tests pass when:
- ✅ Admin cannot access any customer features
- ✅ Customer can access all customer features
- ✅ Appropriate error messages display
- ✅ No application crashes or errors
- ✅ Navigation shows correct links for each role

**Happy Testing! 🎉**
