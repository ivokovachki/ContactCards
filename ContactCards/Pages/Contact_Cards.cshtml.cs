using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;

namespace ContactCards.Pages
{
    public class ContactCardPage : PageModel
    {
        private readonly ILogger<ContactCardPage> _logger;

        private static List<Contact> _contacts = new();
        private static int nextId = 1;

        public ContactCardPage(ILogger<ContactCardPage> logger)
        {
            _logger = logger;
        }

        [BindProperty]
        public Contact NewContact { get; set; } = new();

        [BindProperty]
        public int? EditingContactId { get; set; }

        public string ContactsJson => JsonSerializer.Serialize(_contacts);

        public List<Contact> Contacts => _contacts;

        public void OnGet() { }

        public IActionResult OnGetEdit(int id)
        {
            var contact = _contacts.FirstOrDefault(c => c.Id == id);
            if (contact == null)
                return NotFound();

            NewContact = new Contact
            {
                Name = contact.Name,
                Email = contact.Email,
                Phone = contact.Phone
            };

            EditingContactId = id;
            return Page();
        }

        public IActionResult OnGetDelete(int id)
        {
            var contact = _contacts.FirstOrDefault(c => c.Id == id);
            if (contact != null)
            {
                _contacts.Remove(contact);
            }

            return RedirectToPage();
        }

        public IActionResult OnPost()
        {
            if (EditingContactId.HasValue)
            {
                // Editing existing contact
                if (_contacts.Any(c =>
                    c.Id != EditingContactId.Value &&
                    c.Email.Equals(NewContact.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    ModelState.AddModelError("NewContact.Email", "Email already exists.");
                    return Page();
                }

                if (_contacts.Any(c =>
                    c.Id != EditingContactId.Value &&
                    c.Phone == NewContact.Phone))
                {
                    ModelState.AddModelError("NewContact.Phone", "Phone number already exists.");
                    return Page();
                }

                if (!ModelState.IsValid)
                    return Page();

                var contact = _contacts.FirstOrDefault(c => c.Id == EditingContactId.Value);
                if (contact != null)
                {
                    contact.Name = NewContact.Name;
                    contact.Email = NewContact.Email;
                    contact.Phone = NewContact.Phone;
                }
            }
            else
            {
                // Adding new contact
                if (_contacts.Any(c => c.Email.Equals(NewContact.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    ModelState.AddModelError("NewContact.Email", "Email already exists.");
                    return Page();
                }

                if (_contacts.Any(c => c.Phone == NewContact.Phone))
                {
                    ModelState.AddModelError("NewContact.Phone", "Phone number already exists.");
                    return Page();
                }

                if (!ModelState.IsValid)
                    return Page();

                NewContact.Id = nextId++;
                _contacts.Add(new Contact
                {
                    Id = NewContact.Id,
                    Name = NewContact.Name,
                    Email = NewContact.Email,
                    Phone = NewContact.Phone
                });
            }

            // Reset form
            NewContact = new Contact();
            EditingContactId = null;

            return RedirectToPage();
        }

        public class Contact
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "Name is required.")]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email is required.")]
            [EmailAddress(ErrorMessage = "Invalid email address.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Phone number is required.")]
            [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be 10 digits long.")]
            public string Phone { get; set; } = string.Empty;
        }
    }
}
