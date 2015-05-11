using System;
using System.ComponentModel;
using Catel.Data;
using Catel.MVVM;

namespace TranslateApp.ViewModels {
	public class NewTranslationVariantWindowVM : ViewModelBase {
		
		#region NameRu property

		/// <summary>
		/// Gets or sets the NameRu value.
		/// </summary>
		public string RuName
		{
			get { return GetValue<string>(RuNameProperty); }
			set { SetValue(RuNameProperty, value); }
		}

		/// <summary>
		/// NameRu property data.
		/// </summary>
		public static readonly PropertyData RuNameProperty = RegisterProperty("NameRu", typeof (string));

		#endregion

		#region EnName property

		/// <summary>
		/// Gets or sets the EnName value.
		/// </summary>
		public string EnName
		{
			get { return GetValue<string>(EnNameProperty); }
			set { SetValue(EnNameProperty, value); }
		}

		/// <summary>
		/// EnName property data.
		/// </summary>
		public static readonly PropertyData EnNameProperty = RegisterProperty("EnName", typeof (string));

		#endregion

		public bool ClearAll() {
			RuName = String.Empty;
			EnName = String.Empty;
			return true;
		}

	}
}
