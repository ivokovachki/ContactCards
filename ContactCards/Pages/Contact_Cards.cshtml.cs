using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ContactCards.Data;
using ContactCards.Models;

namespace ContactCards.Pages
{
    public class ContactCardPage : PageModel
    {
        private readonly ILogger<ContactCardPage> _logger;
        private readonly ContactCardsDbContext _context;

        public ContactCardPage(ILogger<ContactCardPage> logger, ContactCardsDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public Contact NewContact { get; set; } = new();

        [BindProperty]
        public int? EditingContactId { get; set; }

        public List<Contact> Contacts { get; set; } = new();

        public async Task OnGetAsync()
        {
            Contacts = await _context.Contacts.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<IActionResult> OnGetEditAsync(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null)
                return NotFound();

            NewContact = new Contact
            {
                Name = contact.Name,
                Email = contact.Email,
                Phone = contact.Phone
            };

            EditingContactId = id;
            Contacts = await _context.Contacts.OrderBy(c => c.Name).ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Load contacts for display in case of validation errors
            Contacts = await _context.Contacts.OrderBy(c => c.Name).ToListAsync();

            if (EditingContactId.HasValue)
            {
                // Update existing contact
                var existingContact = await _context.Contacts.FindAsync(EditingContactId.Value);
                if (existingContact == null)
                {
                    ModelState.AddModelError("", "Contact not found.");
                    return Page();
                }

                // Check for duplicate email (excluding current contact)
                if (await _context.Contacts.AnyAsync(c => c.Email.ToLower() == NewContact.Email.ToLower() && c.Id != EditingContactId.Value))
                {
                    ModelState.AddModelError("NewContact.Email", "Email already exists.");
                    return Page();
                }

                // Check for duplicate phone (excluding current contact)
                if (await _context.Contacts.AnyAsync(c => c.Phone == NewContact.Phone && c.Id != EditingContactId.Value))
                {
                    ModelState.AddModelError("NewContact.Phone", "Phone number already exists.");
                    return Page();
                }

                if (!ModelState.IsValid)
                    return Page();

                existingContact.Name = NewContact.Name;
                existingContact.Email = NewContact.Email;
                existingContact.Phone = NewContact.Phone;

                await _context.SaveChangesAsync();
            }
            else
            {
                // Add new contact
                // Check for duplicate email
                if (await _context.Contacts.AnyAsync(c => c.Email.ToLower() == NewContact.Email.ToLower()))
                {
                    ModelState.AddModelError("NewContact.Email", "Email already exists.");
                    return Page();
                }

                // Check for duplicate phone
                if (await _context.Contacts.AnyAsync(c => c.Phone == NewContact.Phone))
                {
                    ModelState.AddModelError("NewContact.Phone", "Phone number already exists.");
                    return Page();
                }

                if (!ModelState.IsValid)
                    return Page();

                _context.Contacts.Add(NewContact);
                await _context.SaveChangesAsync();
            }

            // Reset form
            NewContact = new Contact();
            EditingContactId = null;

            return RedirectToPage();
        }
    }
}
