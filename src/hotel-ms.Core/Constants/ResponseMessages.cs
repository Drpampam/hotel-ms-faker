namespace hotelier_core_app.Core.Constants
{
    public class ResponseMessages
    {
        public const string DuplicateKeyMessage = "duplicate key";
        public const string UpdateSuccessful = "Record successfully updated";
        public const string UpdateFailed = "Update operation failed. Kindly refer to the StatusCode.";
        public const string InvalidData = "Supplied data is invalid";
        public const string InvalidFile = "Invalid File. Please check file extension";
        public const string OperationSuccessful = "Operation successful!";
        public const string OperationFailed = "Sorry, there was an error processing your request.";
        public const string ReasonRequired = "Reason for declining request is required";
        public const string RequestReasonRequired = "Reason for request is required";
        public const string PartialUpdate = "Update operation failed for some record.";
        public const string UpdateOperationFailed = "Update operation failed. Please update the card status to a valid state.";
        public const string ErrorSendingMessage = "Could not send message. Please try again or contact admin for assistance";

        public const string CachePersisterLogExceptionGotten = "Oops! A error occurred while attempting to log failed cache object to temporal database";
        public const string ErrorPerformingCacheServiceOperation = "Oops! An error occurred while performing cache operation.";

        public const string SQlTransactionNotInitialized = "Oops! An error occurred while processing your request. If this persists after three(3) trials, kindly contact your administrator.";
        public const string InvalidElasticTableName = "Could not locate entity for search. Kindly contact your administrator";
        public const string NoRecordFound = "No record was found";
        public const string GeneralError = "Oops! An error occurred while processing your request. If this persists after three(3) trials, kindly contact your administrator.";
        public const string SQlException = "Oops! A database error occurred while processing your request. If this persists after three(3) trials, kindly contact your administrator.";
        public const string AuditLogObjectEmpty = "Oops! A error occurred while preparing a trail for your request. If this persists after three(3) trials, kindly contact your administrator.";

        //User Management
        public const string UserExist = "User already exist";
        public const string UserCreated = "User created successfully";
        public const string UserDoesNotExist = "User does not exist";
        public const string UserActivated = "User activated";
        public const string UserDeactivated = "User deactivated";
        public const string UserRemoved = "User removed successfully";
        public const string UserInactive = "This account is currently not active, contact Admin for help";
        public const string LoginSuccessful = "Login successful";
        public const string CantVerifyToken = "Can't Verify Token";
        public const string CantVerifyRefreshToken = "Can't Verify Refresh Token";
        public const string UsersRetrieved = "Users Retrieved successfully";
        public const string UsersFetchFailed = "User Fetch Failed";

        //User Role
        public const string RoleExist = "Role with this name already exist";
        public const string RoleNotExist = "This role does not already exist";
        public const string RoleCreated = "Role created successfully";
        public const string RoleUpdated = "Role updated successfully";
        public const string RoleReassignmentError = "Role reassignment error";

        //Module Service
        public const string ModuleGroupUpdated = "Dashboard detail is updated";
        public const string ModuleGroupNotExist = "Module Group does not exist";
        public const string ModuleGroupUpdateValidation = "Any of the module group field is required";
        public const string ModuleUpdated = "Module detail is updated";
        public const string ModuleNotExist = "Module does not exist";
        public const string ModuleGroupExist = "Module group already exist";
        public const string ModuleExist = "Module already exist";
        public const string ModuleUpdateValidation = "Either of the module field is required";
        public const string NoModuleAccess = "No module access found for user";
        public const string ModulesRetrieved = "Modules Retrieved successfully";

        public const string EmailSent = "An email has been successfully sent to the user";
        public const string EmailFailed = "Failed to send the email. Please try again later";

        public const string InvalidCredential = "Invalid Credentials";
        public const string UserEmailNotConfirmed = "User's email has not been confirmed";

        // Subscription service
        public const string SubscriptionExist = "Subscription already exist";
        public const string SubscriptionNotExist = "Subscription does not exist";
        public const string SubscriptionCreated = "Subscription created successfully";
        public const string SubscriptionUpdated = "Subscription updated successfully";
        public const string SubscriptionRemoved = "Subscription removed successfully";
        public const string Subscribed = "Subscribed to plan successfully";

        // Policy Group Management
        public const string PolicyGroupExists = "A policy with this name already exists for this tenant";
        public const string PolicyGroupDoesNotExist = "The specified policy group does not exist";
        public const string UserNotInPolicyGroup = "The user is not in the specified policy group";
        public const string PermissionDoesNotExist = "The specified permission does not exist";
        public const string PolicyDoesNotExist = "The specified policy does not exist";

        // Property Management
        public const string TenantNotExisting = "Tenant does not exist";
        public const string UserNotInTenant = "The user does not belong to the tenant specified";
        public const string PropertyNotFound = "Property not found";

        // Room Management
        public const string RoomNotFound = "Room not found";
        public const string RoomCreated = "Room created successfully";
        public const string RoomUpdated = "Room updated successfully";
        public const string RoomDeleted = "Room deleted successfully";
        public const string RoomAlreadyOccupied = "Room is not available for the selected dates";
        public const string RoomsRetrieved = "Rooms retrieved successfully";

        // Payment Management
        public const string PaymentNotFound = "Payment not found";
        public const string PaymentCreated = "Payment created successfully";
        public const string PaymentsRetrieved = "Payments retrieved successfully";
        public const string ReservationNotFound = "Reservation not found";

        // Service Request Management
        public const string ServiceRequestNotFound = "Service request not found";
        public const string ServiceRequestCreated = "Service request created successfully";
        public const string ServiceRequestUpdated = "Service request updated successfully";
        public const string ServiceRequestsRetrieved = "Service requests retrieved successfully";

        // Discount Management
        public const string DiscountNotFound = "Discount not found";
        public const string DiscountCreated = "Discount created successfully";
        public const string DiscountUpdated = "Discount updated successfully";
        public const string DiscountDeleted = "Discount deleted successfully";
        public const string DiscountExists = "A discount with this name already exists";
        public const string DiscountsRetrieved = "Discounts retrieved successfully";
        public const string DiscountExpired = "This discount has expired";
        public const string DiscountNotActive = "This discount is not active";

        // Reservation Management
        public const string ReservationCreated = "Reservation created successfully";
        public const string ReservationUpdated = "Reservation updated successfully";
        public const string ReservationCancelled = "Reservation cancelled successfully";
        public const string ReservationCheckedIn = "Guest checked in successfully";
        public const string ReservationCheckedOut = "Guest checked out successfully";
        public const string ReservationsRetrieved = "Reservations retrieved successfully";
        public const string InvalidDateRange = "Check-out date must be after check-in date";
        public const string RoomNotAvailable = "The selected room is not available for the chosen dates";
        public const string ReservationNotCancellable = "Only pending or confirmed reservations can be cancelled";
        public const string ReservationNotCheckInable = "Only confirmed reservations can be checked in";
        public const string ReservationNotCheckOutable = "Only checked-in reservations can be checked out";
        public const string ReservationExpired = "Cannot check in: the reservation check-out date has already passed";
        public const string ReservationDatesLocked = "Reservation dates and room cannot be changed after check-in";

        // Guest Management
        public const string GuestProfileNotFound = "Guest profile not found";
        public const string GuestProfileCreated = "Guest profile created successfully";
        public const string GuestProfileUpdated = "Guest profile updated successfully";
        public const string GuestProfileAlreadyExists = "A guest profile already exists for this user";
        public const string GuestsRetrieved = "Guests retrieved successfully";

        // Housekeeping
        public const string HousekeepingTaskNotFound = "Housekeeping task not found";
        public const string HousekeepingTaskCreated = "Housekeeping task created successfully";

        // Billing
        public const string InvoiceNotFound = "Invoice not found";
        public const string InvoiceGenerated = "Invoice generated successfully";
        public const string InvoiceVoided = "Invoice voided successfully";
        public const string InvoiceAlreadyVoided = "This invoice has already been voided";
        public const string InvoiceAlreadyPaid = "This invoice has already been paid";
        public const string InvoicesRetrieved = "Invoices retrieved successfully";

        // Loyalty
        public const string LoyaltyRecordNotFound = "Loyalty record not found";
        public const string InsufficientLoyaltyPoints = "Insufficient loyalty points for redemption";
        public const string LoyaltyPointsRedeemed = "Loyalty points redeemed successfully";

        // Reporting
        public const string ReportGenerated = "Report generated successfully";
    }
}
