﻿using Windows.Graphics;
using Microsoft.UI;
using Microsoft.UI.Windowing;

namespace HuntStats;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        
        MainPage = new MainPage();
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);
        if (window != null) window.Title = "HüntStäts by Zaxiure";

        return window;
    }
}