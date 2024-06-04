 if (Screen.AllScreens.Length <= 1) return;
                var screens = Screen.AllScreens;
                var bounds = screens[1].Bounds;
                frmsecScreen.SetBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height);
                frmsecScreen.StartPosition = FormStartPosition.center;
                frmsecScreen.Show();