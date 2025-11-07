// Tenant Themes Configuration

export enum TenantTheme {
  Blue = 0,
  Green = 1,
  Purple = 2,
  Orange = 3,
  Navy = 4,
  Red = 5
}

export interface ThemeConfig {
  id: TenantTheme;
  name: string;
  description: string;
  colors: {
    primary: string;      // Hauptfarbe
    primaryHover: string; // Hover-Zustand
    primaryLight: string; // Helle Variante
    accent: string;       // Akzentfarbe
    text: string;         // Textfarbe auf Primary
  };
}

export const themes: Record<TenantTheme, ThemeConfig> = {
  [TenantTheme.Blue]: {
    id: TenantTheme.Blue,
    name: 'Blau - Standard',
    description: 'Klassisches Blau, vertrauenswürdig und professionell',
    colors: {
      primary: 'rgb(37, 99, 235)',      // blue-600
      primaryHover: 'rgb(29, 78, 216)', // blue-700
      primaryLight: 'rgb(96, 165, 250)', // blue-400
      accent: 'rgb(59, 130, 246)',      // blue-500
      text: 'rgb(255, 255, 255)'
    }
  },
  [TenantTheme.Green]: {
    id: TenantTheme.Green,
    name: 'Grün - Nachhaltig',
    description: 'Umweltfreundlich, nachhaltig und natürlich',
    colors: {
      primary: 'rgb(22, 163, 74)',      // green-600
      primaryHover: 'rgb(21, 128, 61)', // green-700
      primaryLight: 'rgb(74, 222, 128)', // green-400
      accent: 'rgb(34, 197, 94)',       // green-500
      text: 'rgb(255, 255, 255)'
    }
  },
  [TenantTheme.Purple]: {
    id: TenantTheme.Purple,
    name: 'Lila - Modern',
    description: 'Modern, innovativ und kreativ',
    colors: {
      primary: 'rgb(147, 51, 234)',     // purple-600
      primaryHover: 'rgb(126, 34, 206)', // purple-700
      primaryLight: 'rgb(192, 132, 252)', // purple-400
      accent: 'rgb(168, 85, 247)',      // purple-500
      text: 'rgb(255, 255, 255)'
    }
  },
  [TenantTheme.Orange]: {
    id: TenantTheme.Orange,
    name: 'Orange - Energiegeladen',
    description: 'Dynamisch, energiegeladen und enthusiastisch',
    colors: {
      primary: 'rgb(234, 88, 12)',      // orange-600
      primaryHover: 'rgb(194, 65, 12)', // orange-700
      primaryLight: 'rgb(251, 146, 60)', // orange-400
      accent: 'rgb(249, 115, 22)',      // orange-500
      text: 'rgb(255, 255, 255)'
    }
  },
  [TenantTheme.Navy]: {
    id: TenantTheme.Navy,
    name: 'Dunkelblau - Professionell',
    description: 'Professionell, vertrauenswürdig und seriös',
    colors: {
      primary: 'rgb(30, 58, 138)',      // blue-900
      primaryHover: 'rgb(23, 37, 84)',  // blue-950
      primaryLight: 'rgb(59, 130, 246)', // blue-500
      accent: 'rgb(37, 99, 235)',       // blue-600
      text: 'rgb(255, 255, 255)'
    }
  },
  [TenantTheme.Red]: {
    id: TenantTheme.Red,
    name: 'Rot - Kraftvoll',
    description: 'Kraftvoll, aufmerksamkeitsstark und energisch',
    colors: {
      primary: 'rgb(220, 38, 38)',      // red-600
      primaryHover: 'rgb(185, 28, 28)', // red-700
      primaryLight: 'rgb(248, 113, 113)', // red-400
      accent: 'rgb(239, 68, 68)',       // red-500
      text: 'rgb(255, 255, 255)'
    }
  }
};

// Helper function to apply theme
export const applyTheme = (theme: TenantTheme) => {
  const themeConfig = themes[theme];
  const root = document.documentElement;
  
  root.style.setProperty('--color-primary', themeConfig.colors.primary);
  root.style.setProperty('--color-primary-hover', themeConfig.colors.primaryHover);
  root.style.setProperty('--color-primary-light', themeConfig.colors.primaryLight);
  root.style.setProperty('--color-accent', themeConfig.colors.accent);
  root.style.setProperty('--color-primary-text', themeConfig.colors.text);
};

// Get theme from user data
export const getCurrentTheme = (): TenantTheme => {
  try {
    const userStr = localStorage.getItem('user');
    if (userStr) {
      const user = JSON.parse(userStr);
      // TODO: Add theme to user object from backend
      return user.theme !== undefined ? user.theme : TenantTheme.Blue;
    }
  } catch (error) {
    console.error('Error getting theme:', error);
  }
  return TenantTheme.Blue;
};


