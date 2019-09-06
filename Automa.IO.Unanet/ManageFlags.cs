using System;

namespace Automa.IO.Unanet
{
    [Flags]
    public enum ManageFlags
    {
        None = 0x0,
        Default = AllEntity | Export,

        // OPERATIONS
        Export = 0x1,
        Sync = 0x2,

        //AllEntity = Assignment,
        AllEntity = OrganizationAll | ProjectAll | PersonAll | TimeInvoiceAll,
        // Entity:Organization
        OrganizationMask = 0x1F0,
        OrganizationAll = Organization | CustomerProfile | OrganizationAddress, //| OrganizationContact,
        Organization = 0x110,
        CustomerProfile = 0x120,
        OrganizationAddress = 0x140,
        OrganizationContact = 0x180,
        // Entity:Project
        ProjectMask = 0x6F0,
        ProjectAll = Project | ProjectLaborCategory | Task | FixedPrice | Assignment,
        Project = 0x210,
        Task = 0x220,
        ProjectLaborCategory = 0x240,
        FixedPrice = 0x280,
        Assignment = 0x400,
        ProjectAdministrator = 0x410,
        // Entity:Person
        PersonMask = 0x8F0,
        PersonAll = Person | ApprovalGroups,
        Person = 0x810,
        ApprovalGroups = 0x820,
        // Entity:Time
        TimeInvoiceMask = 0x10F0,
        TimeInvoiceAll = Time | Invoice,
        Time = 0x1010,
        Invoice = 0x1020,

        // Changed:Organization
        OrganizationChanged = Organization | Export,
        CustomerProfileChanged = CustomerProfile | Export,
        OrganizationAddressChanged = OrganizationAddress | Export,
        OrganizationContactChanged = OrganizationContact | Export,
        // Changed:Project
        ProjectChanged = Project | Export,
        TaskChanged = Task | Export,
        FixedPriceChanged = FixedPrice | Export,
        AssignmentChanged = Assignment | Export,
        ProjectAdministratorChanged = ProjectAdministrator | Export,
        ProjectLaborCategoryChanged = ProjectLaborCategory | Export,
        // Changed:Person
        PersonChanged = Person | Export,
        ApprovalGroupChanged = ApprovalGroups | Export,
    }
}