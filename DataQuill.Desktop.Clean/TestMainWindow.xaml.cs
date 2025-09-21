using System;
using System.Windows;
using DataQuillDesktop.Services;
using DataQuillDesktop.ViewModels;

namespace DataQuillDesktop
{
    public partial class TestMainWindow : Window
    {
        public TestMainWindow()
        {
            InitializeComponent();
            Title = "DataQuill Test Window";
            Width = 800;
            Height = 600;

            // Simple test with fallback viewmodel
            DataContext = new FallbackViewModel();
            Console.WriteLine("Test window initialized with FallbackViewModel");
        }
    }
}