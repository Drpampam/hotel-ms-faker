namespace hotelier_core_app.Core.Constants
{
    public class UserAction
    {
        //User management
        public const string CreateUser = "Create User";
        public const string EditUser = "Edit User";
        public const string ActivateUser = "Activate User";
        public const string DeactivateUser = "Deactivate User";
        public const string UserLogin = "User Login";
        public const string RefreshToken = "Refresh Token";

        //Role management
        public const string CreateUserRole = "Create User Role";
        public const string EditUserRole = "Edit User Role";
        public const string DeleteUserRole = "Delete User Role";
        public const string ReassignRole = "Reassign Role";

        //Module Management
        public const string CreateModuleGroup = "Create Module Group";
        public const string EditModuleGroup = "Edit Module Group";
        public const string CreateModule = "Create Module";
        public const string EditModule = "Edit Module";

        //SubscriptionPlan management
        public const string CreateSubscriptionPlan = "Create Subscription Plan";
        public const string EditSubscriptionPlan = "Edit Subscription Plan";
        public const string DeleteSubscriptionPlan = "Delete Subscription Plan";
        public const string ActivateSubscriptionPlan = "Activate Subscription Plan";
        public const string DeactivateSubscriptionPlan = "Deactivate Subscription Plan";

        // Property Management
        public const string AddProperty = "Add Property";
        public const string UpdateProperty = "Update Property";

        // Room Management
        public const string AddRoom = "Add Room";
        public const string UpdateRoom = "Update Room";
        public const string DeleteRoom = "Delete Room";

        // Payment Management
        public const string CreatePayment = "Create Payment";
        public const string CapturePayment = "Capture Payment";

        // Service Request Management
        public const string CreateServiceRequest = "Create Service Request";
        public const string UpdateServiceRequest = "Update Service Request";

        // Discount Management
        public const string CreateDiscount = "Create Discount";
        public const string UpdateDiscount = "Update Discount";
        public const string DeleteDiscount = "Delete Discount";

        // Reservation Management
        public const string CreateReservation = "Create Reservation";
        public const string UpdateReservation = "Update Reservation";
        public const string CancelReservation = "Cancel Reservation";
        public const string CheckIn = "Check In";
        public const string CheckOut = "Check Out";
        public const string OverrideReservationStatus = "Override Reservation Status";

        // Guest Management
        public const string CreateGuestProfile = "Create Guest Profile";
        public const string UpdateGuestProfile = "Update Guest Profile";

        // Housekeeping
        public const string CreateHousekeepingTask = "Create Housekeeping Task";
        public const string UpdateHousekeepingTask = "Update Housekeeping Task";

        // Billing
        public const string GenerateInvoice = "Generate Invoice";
        public const string VoidInvoice = "Void Invoice";

        // Reservation Expenses
        public const string AddReservationExpense = "Add Reservation Expense";
        public const string DeleteReservationExpense = "Delete Reservation Expense";

        // Loyalty
        public const string RedeemLoyaltyPoints = "Redeem Loyalty Points";
    }
}
