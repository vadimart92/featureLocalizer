namespace TranslateApp {
	using Catel.Windows;
	using ViewModels;

	public partial class DataWindow1 {
		public DataWindow1()
			: this(null) {
		}

		public DataWindow1(DataWindow1Model viewModel)
			: base(viewModel) {
			InitializeComponent();
		}
	}
}
