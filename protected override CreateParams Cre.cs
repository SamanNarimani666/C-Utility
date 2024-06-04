  protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                if (!DesignMode)
                    cp.ExStyle |= 0x02000000;
                return cp;
            }
        }
