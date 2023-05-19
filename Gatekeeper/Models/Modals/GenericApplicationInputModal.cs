using Discord.Interactions;

namespace Gatekeeper.Models.Modals
{
    public class GenericApplicationInputModal : IModal
    {
        public string Title => string.Empty;

        [ModalTextInput("first")]
        public string First { get; set; }

        [ModalTextInput("second")]
        public string Second { get; set; }

        [ModalTextInput("third")]
        public string Third { get; set; }

        [ModalTextInput("fourth")]
        public string Fourth { get; set; }
    }
}
