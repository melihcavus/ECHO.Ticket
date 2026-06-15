import React, { createContext, useContext, useState } from 'react';

const translations = {
    tr: {},
    en: {
        // --- My Tickets ---
        "myTicketsTitle": "My Tickets & Pledges",
        "myTicketsDesc": "All your purchased packages and supported projects are here.",
        "loadingTickets": "Loading your tickets...",
        "noTicketsTitle": "No tickets yet",
        "noTicketsDesc": "You can get your first ticket by supporting projects in the ECHO world.",
        "exploreProjectsBtn": "Explore Projects",
        "transactionId": "Transaction ID",
        "paidAmount": "Amount Paid",
        "ticketsLoadError": "Tickets could not be loaded.",
        "serverConnError": "Server connection error.",

        // --- Dashboard ---
        "authError": "User identity not found. Please log in again.",
        "fetchError": "Could not fetch data",
        "statsLoadError": "Could not load statistics.",
        "welcomeBack": "Welcome back",
        "dashboardSubtitle": "Here is what's happening with your projects and tickets today.",
        "loadingStats": "Loading statistics...",
        "totalPledge": "Total Pledged",
        "thisMonth": "this month",
        "activeProjects": "Active Projects",
        "campaignsEndingSoon": "campaigns ending soon",
        "upcomingEvents": "Upcoming Events",
        "ticket": "Ticket",
        "nextEventIn": "Next event in 5 days",
        "recentActivities": "Recent Activities",
        "noRecentActivity": "No Recent Activity",
        "noActivityDesc": "You haven't supported any campaigns or bought tickets recently. Explore the platform to get started!",
        "exploreCampaignsBtn": "Explore Campaigns",

        // --- Explore & EventDetail ---
        "eventsFetchError": "Could not fetch events",
        "campaignsLoadError": "Could not load campaigns.",
        "eventCreated": "Event successfully created!",
        "exploreCampaigns": "Explore Campaigns",
        "exploreDesc": "Check out world-changing projects and exciting events.",
        "newCampaign": "New Campaign",
        "searchPlaceholder": "Search campaigns, tickets...",
        "loadingProjects": "Loading projects...",
        "noActiveCampaigns": "There are no active campaigns at the moment.",
        "active": "Active",
        "totalPledged": "Total Pledged",
        "viewProject": "View Project",
        "startNewCampaign": "Start New Campaign",
        "campaignTitle": "Campaign Title",
        "campaignTitlePlaceholder": "e.g. Next Gen Robotic Arm",
        "category": "Category",
        "location": "Location",
        "locationPlaceholder": "e.g. Istanbul / Online",
        "eventEndDate": "Event/End Date",
        "campaignDescPlaceholder": "Explain your project in detail...",
        "createCampaign": "Create Campaign",
        "loadError": "An error occurred while loading the event.",
        "loginRequired": "You must be logged in to purchase.",
        "insufficientBalance": "Insufficient balance!",
        "ticketPrice": "Ticket Price",
        "balancePrompt": "Please top up your wallet to continue.",
        "purchaseSuccess": "Your purchase has been queued! It will reflect here once stock is updated.",
        "operationFailed": "Operation failed",
        "unknownError": "An unknown error occurred.",
        "serverError": "Could not connect to the server during the transaction.",
        "packageAdded": "Package added successfully!",
        "error": "Error",
        "createFailed": "Creation failed",
        "serverComError": "A server communication error occurred.",
        "loadingDetails": "Loading details...",
        "activeCampaign": "Active Campaign",
        "aboutProject": "About Project",
        "noDescription": "No detailed description has been provided for this project yet.",
        "supportPackages": "Support Packages",
        "addPackage": "Add Package",
        "stock": "Stock",
        "processing": "Processing...",
        "buyTicket": "Support / Buy Ticket",
        "soldOut": "Sold Out",
        "noPackages": "No packages added for this event yet.",
        "createPackage": "Create New Package",
        "packageName": "Package Name",
        "packagePlaceholder": "e.g. VIP Attendance Package",
        "price": "Price (₺)",
        "capacity": "Capacity (Stock)",
        "description": "Description",
        "descPlaceholder": "Write the package benefits...",
        "creating": "Creating...",
        "savePackage": "Save Package",

        // --- Settings ---
        "profileUpdateSuccess": "Profile updated successfully!",
        "profileUpdateError": "Profile could not be updated.",
        "passwordsDoNotMatch": "New passwords do not match!",
        "passwordUpdateSuccess": "Password updated successfully!",
        "passwordUpdateError": "Password could not be updated.",
        "accountSettingsTitle": "Account Settings",
        "accountSettingsDesc": "Manage your profile and security preferences here.",
        "profileInfo": "Profile Information",
        "securityAndPass": "Security & Password",
        "profilePhoto": "Profile Photo",
        "profilePhotoDesc": "Initials from your registered name are assigned automatically.",
        "firstName": "First Name",
        "lastName": "Last Name",
        "emailAddr": "Email Address",
        "emailNotice": "Email address cannot be changed for security reasons.",
        "saveChanges": "Save Changes",
        "changePasswordTitle": "Change Password",
        "currentPassword": "Current Password",
        "newPassword": "New Password",
        "newPasswordConfirm": "New Password (Confirm)",
        "updatePasswordBtn": "Update Password",

        // --- Wallet ---
        "enterValidAmount": "Please enter a valid amount.",
        "balanceLoadSuccess": "Balance loaded successfully!",
        "balanceLoadFailed": "Loading failed.",
        "myWalletTitle": "My Wallet",
        "myWalletDesc": "Manage your ECHO platform balance and support projects seamlessly.",
        "echoWallet": "ECHO WALLET",
        "currentBalance": "Current Balance",
        "loadBalanceTitle": "Load Balance",
        "amountToLoad": "Amount to Load (₺)",
        "securePaymentBtn": "Make Secure Payment",

        // --- Venues ---
        "venuesMenu": "Venues",
        "venuesTitle": "Venues & Seating Plans",
        "venuesDesc": "Manage all event venues and seating grids on the platform.",
        "addNewVenue": "Add New Venue",
        "venueName": "Venue Name",
        "venueNamePlaceholder": "e.g. Open Air Theatre",
        "rowCount": "Row Count (Letters)",
        "colCount": "Column Count (Numbers)",
        "saveVenue": "Save Venue",
        "totalCapacity": "Total Capacity",
        "seats": "seats",
        "venueAddedSuccess": "Venue added successfully!",
        "venuesLoadError": "Could not load venues.",

        "venueSelect": "Venue / Stage (Optional)",
        "noVenue": "No Venue / Standing / Online",
        "selectSeatAlert": "Please select a seat from the map first!",
        "seatSelection": "Seat Selection",
        "seatsSold": "Seats Sold",
        "stage": "STAGE",
        "seatTaken": "Taken",
        "availableSeat": "Available",
        "takenSeat": "Taken",
        "selectedSeat": "Selected",

        // --- Search ---
        "searchPlaceholder": "Search campaigns, tickets, categories or locations...",
        "noMatchingEvents": "No events found matching your search criteria.",
        "searchTicketsPlaceholder": "Search by event, ticket type or transaction ID...",
        "noMatchingTickets": "No tickets found matching your search criteria.",

        // --- Reviews ---
        "reviewCount": "reviews",
        "reviewsAndRatings": "Reviews and Ratings",
        "writeReview": "Rate and Review",
        "reviewPlaceholder": "Share your thoughts about the event...",
        "sending": "Sending...",
        "sendReview": "Submit Review",
        "loginToReview": "Please log in to leave a review and rating.",
        "noReviewsYet": "No reviews have been made for this event yet. Be the first to rate!",
        "invalidRating": "Rating must be between 1 and 5.",
        "reviewAddedSuccess": "Your review was added successfully!"
    }
};

const LanguageContext = createContext();

export const useLanguage = () => useContext(LanguageContext);

export const LanguageProvider = ({ children }) => {
    const [lang, setLang] = useState(() => localStorage.getItem('lang') || 'tr');

    const toggleLanguage = () => {
        const newLang = lang === 'tr' ? 'en' : 'tr';
        setLang(newLang);
        localStorage.setItem('lang', newLang);
    };

    const t = (key, defaultText) => {
        return translations[lang]?.[key] || defaultText;
    };

    return (
        <LanguageContext.Provider value={{ lang, toggleLanguage, t }}>
            {children}
        </LanguageContext.Provider>
    );
};