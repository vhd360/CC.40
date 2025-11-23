import React, { useEffect, useState } from 'react';
import { Outlet, Link, useLocation } from 'react-router-dom';
import { Avatar, AvatarFallback } from './ui/avatar';
import { Button } from './ui/button';
import { ThemeToggle } from './ThemeToggle';
import {
  BarChart3,
  Users,
  Building2,
  Zap,
  Layers,
  UsersRound,
  Key,
  Car,
  CreditCard,
  QrCode,
  Settings,
  Menu,
  X,
  DollarSign,
  UserCog,
  ChevronLeft,
  ChevronRight,
  LogOut
} from 'lucide-react';
import { applyTheme, TenantTheme } from '../themes/tenantThemes';

const navigation = [
  { name: 'Dashboard', href: '/', icon: BarChart3 },
  { name: 'Einstellungen', href: '/settings', icon: Settings },
  { name: 'Tenants', href: '/tenants', icon: Building2 },
  { name: 'Ladeparks', href: '/charging-parks', icon: Building2 },
  { name: 'Ladestationen', href: '/charging-stations', icon: Zap },
  { name: 'Ladepunkt-Gruppen', href: '/charging-station-groups', icon: Layers },
  { name: 'Benutzer', href: '/users', icon: Users },
  { name: 'Nutzergruppen', href: '/user-groups', icon: UsersRound },
  { name: 'Identifikationsmethoden', href: '/authorization-methods', icon: Key },
  { name: 'Fahrzeuge', href: '/vehicles', icon: Car },
  { name: 'Fahrzeugzuweisungen', href: '/vehicle-assignments', icon: UserCog },
  { name: 'Tarife', href: '/tariffs', icon: DollarSign },
  { name: 'Abrechnung', href: '/billing', icon: CreditCard },
  { name: 'QR-Codes', href: '/qrcodes', icon: QrCode },
];

interface AdminLayoutProps {
  onLogout: () => void;
}

export const AdminLayout: React.FC<AdminLayoutProps> = ({ onLogout }) => {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [sidebarCollapsed, setSidebarCollapsed] = useState(() => {
    const saved = localStorage.getItem('sidebarCollapsed');
    return saved === 'true';
  });
  const location = useLocation();
  
  // Speichere Sidebar-Status
  useEffect(() => {
    localStorage.setItem('sidebarCollapsed', sidebarCollapsed.toString());
  }, [sidebarCollapsed]);
  
  // Get user and tenant info from localStorage
  const getUserInfo = () => {
    try {
      const userStr = localStorage.getItem('user');
      if (userStr) {
        const user = JSON.parse(userStr);
        return {
          name: `${user.firstName} ${user.lastName}`,
          email: user.email,
          role: user.role,
          logoUrl: user.tenantLogoUrl,
          theme: user.tenantTheme !== undefined ? user.tenantTheme : TenantTheme.Blue,
          tenantName: user.tenantName || 'CUBOS.Charge'
        };
      }
    } catch (error) {
      console.error('Error parsing user info:', error);
    }
    return { 
      name: 'User', 
      email: '', 
      role: 'User',
      logoUrl: null,
      theme: TenantTheme.Blue,
      tenantName: 'CUBOS.Charge'
    };
  };
  
  const userInfo = getUserInfo();
  const tenantName = userInfo.tenantName;
  const logoUrl = userInfo.logoUrl;
  const theme = userInfo.theme;
  
  // Apply theme on mount and when it changes
  useEffect(() => {
    applyTheme(theme);
  }, [theme]);

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-950">
      {/* Mobile sidebar */}
      <div className={`fixed inset-0 z-50 lg:hidden ${sidebarOpen ? 'block' : 'hidden'}`}>
        <div className="fixed inset-0 bg-black bg-opacity-25" onClick={() => setSidebarOpen(false)} />
        <div className="fixed left-0 top-0 bottom-0 w-64 bg-white dark:bg-gray-900 shadow-lg flex flex-col">
          <div className="flex items-center justify-between p-4 border-b dark:border-gray-800">
            <div className="flex items-center space-x-2">
              {logoUrl ? (
                <img src={`http://localhost:5126${logoUrl}`} alt={`${tenantName} Logo`} className="h-6 w-auto object-contain" />
              ) : (
                <Zap className="h-6 w-6" style={{ color: 'var(--color-primary)' }} />
              )}
              <h2 className="text-lg font-semibold text-gray-900 dark:text-gray-100">{tenantName}</h2>
            </div>
            <Button variant="ghost" size="sm" onClick={() => setSidebarOpen(false)}>
              <X className="h-4 w-4" />
            </Button>
          </div>
          
          <nav className="flex-1 overflow-y-auto p-4">
            <ul className="space-y-2">
              {navigation.map((item) => (
                <li key={item.name}>
                  <Link
                    to={item.href}
                    className={`flex items-center space-x-3 px-3 py-2 rounded-md text-sm font-medium transition-colors ${
                      location.pathname === item.href
                        ? 'bg-primary/10 text-primary hover:bg-primary/20'
                        : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-800 hover:text-gray-900 dark:hover:text-gray-100'
                    }`}
                    onClick={() => setSidebarOpen(false)}
                  >
                    <item.icon className="h-4 w-4" />
                    <span>{item.name}</span>
                  </Link>
                </li>
              ))}
              <li>
                <Link
                  to="/profile"
                  className={`flex items-center space-x-3 px-3 py-2 rounded-md text-sm font-medium transition-colors ${
                    location.pathname === '/profile'
                      ? 'bg-primary/10 text-primary hover:bg-primary/20'
                      : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-800 hover:text-gray-900 dark:hover:text-gray-100'
                  }`}
                  onClick={() => setSidebarOpen(false)}
                >
                  <UserCog className="h-4 w-4" />
                  <span>Profil</span>
                </Link>
              </li>
            </ul>
          </nav>
          
          {/* User Section Mobile */}
          <div className="border-t border-gray-200 dark:border-gray-800 p-4">
            <div className="space-y-3">
              <div className="flex items-center space-x-2">
                <Avatar className="h-8 w-8">
                  <AvatarFallback className="text-xs">{userInfo.name.charAt(0)}</AvatarFallback>
                </Avatar>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium text-gray-900 dark:text-gray-100 truncate">
                    {userInfo.name}
                  </p>
                  <p className="text-xs text-gray-500 dark:text-gray-400 truncate">{userInfo.email}</p>
                </div>
                <ThemeToggle />
              </div>
              <Button variant="outline" size="sm" className="w-full" onClick={onLogout}>
                <LogOut className="h-4 w-4 mr-2" />
                Abmelden
              </Button>
            </div>
          </div>
        </div>
      </div>

      {/* Mobile Header für Toggle */}
      <header className="lg:hidden bg-white dark:bg-gray-900 shadow-sm border-b dark:border-gray-800 sticky top-0 z-40">
        <div className="w-full px-3 py-3">
          <div className="flex justify-between items-center">
            <Button
              variant="ghost"
              size="sm"
              onClick={() => setSidebarOpen(true)}
            >
              <Menu className="h-5 w-5" />
            </Button>
            <div className="flex items-center space-x-2">
              {logoUrl ? (
                <img src={`http://localhost:5126${logoUrl}`} alt={`${tenantName} Logo`} className="h-6 w-auto object-contain" />
              ) : (
                <Zap className="h-6 w-6" style={{ color: 'var(--color-primary)' }} />
              )}
              <h1 className="text-lg font-bold text-gray-900 dark:text-gray-100">{tenantName}</h1>
            </div>
            <div className="w-10"></div>
          </div>
        </div>
      </header>

      <div className="flex min-h-screen lg:min-h-screen">
        {/* Desktop sidebar */}
        <aside className={`hidden lg:flex lg:flex-col lg:fixed lg:inset-y-0 lg:left-0 lg:z-30 transition-all duration-300 ${
          sidebarCollapsed ? 'lg:w-16' : 'lg:w-64'
        }`}>
          <div className="flex flex-col h-full bg-white dark:bg-gray-900 border-r border-gray-200 dark:border-gray-800">
            {/* Logo und Toggle */}
            <div className={`flex items-center border-b dark:border-gray-800 p-4 ${
              sidebarCollapsed ? 'justify-center' : 'justify-between'
            }`}>
              {!sidebarCollapsed && (
                <div className="flex items-center space-x-2">
                  {logoUrl ? (
                    <img src={`http://localhost:5126${logoUrl}`} alt={`${tenantName} Logo`} className="h-7 w-auto object-contain" />
                  ) : (
                    <Zap className="h-7 w-7" style={{ color: 'var(--color-primary)' }} />
                  )}
                  <h1 className="text-lg font-bold text-gray-900 dark:text-gray-100">{tenantName}</h1>
                </div>
              )}
              {sidebarCollapsed && (
                logoUrl ? (
                  <img src={`http://localhost:5126${logoUrl}`} alt={`${tenantName} Logo`} className="h-7 w-auto object-contain" />
                ) : (
                  <Zap className="h-7 w-7" style={{ color: 'var(--color-primary)' }} />
                )
              )}
              <Button
                variant="ghost"
                size="sm"
                onClick={() => setSidebarCollapsed(!sidebarCollapsed)}
                className="text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
              >
                {sidebarCollapsed ? (
                  <ChevronRight className="h-4 w-4" />
                ) : (
                  <ChevronLeft className="h-4 w-4" />
                )}
              </Button>
            </div>

            {/* Navigation */}
            <div className="flex-1 overflow-y-auto px-2 py-4">
              <nav className="space-y-1">
                {navigation.map((item) => (
                  <Link
                    key={item.name}
                    to={item.href}
                    className={`group relative flex items-center px-3 py-2 rounded-md text-sm font-medium transition-colors ${
                      location.pathname === item.href
                        ? 'bg-primary/10 text-primary hover:bg-primary/20'
                        : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-800 hover:text-gray-900 dark:hover:text-gray-100'
                    } ${sidebarCollapsed ? 'justify-center' : 'space-x-3'}`}
                    title={sidebarCollapsed ? item.name : undefined}
                  >
                    <item.icon className="h-4 w-4 flex-shrink-0" />
                    {!sidebarCollapsed && <span className="truncate">{item.name}</span>}
                    
                    {/* Tooltip */}
                    {sidebarCollapsed && (
                      <div className="absolute left-full ml-2 px-3 py-2 bg-gray-900 dark:bg-gray-800 text-white text-sm rounded-md opacity-0 group-hover:opacity-100 pointer-events-none whitespace-nowrap z-50 transition-opacity shadow-lg">
                        {item.name}
                        <div className="absolute left-0 top-1/2 -translate-x-1 -translate-y-1/2 w-2 h-2 bg-gray-900 dark:bg-gray-800 rotate-45"></div>
                      </div>
                    )}
                  </Link>
                ))}
              </nav>
            </div>

            {/* User Section */}
            <div className="border-t border-gray-200 dark:border-gray-800 p-4">
              {!sidebarCollapsed ? (
                <div className="space-y-3">
                  <div className="flex items-center space-x-2">
                    <Link
                      to="/profile"
                      className="flex items-center space-x-2 flex-1 hover:bg-gray-100 dark:hover:bg-gray-800 p-2 rounded-md transition-colors"
                    >
                      <Avatar className="h-8 w-8">
                        <AvatarFallback className="text-xs">{userInfo.name.charAt(0)}</AvatarFallback>
                      </Avatar>
                      <div className="flex-1 min-w-0">
                        <p className="text-sm font-medium text-gray-900 dark:text-gray-100 truncate">
                          {userInfo.name}
                        </p>
                        <p className="text-xs text-gray-500 dark:text-gray-400 truncate">{userInfo.email}</p>
                      </div>
                    </Link>
                    <ThemeToggle />
                  </div>
                  <Button variant="outline" size="sm" className="w-full" onClick={onLogout}>
                    Abmelden
                  </Button>
                </div>
              ) : (
                <div className="flex flex-col items-center space-y-3">
                  <Link to="/profile" title={userInfo.name}>
                    <Avatar className="h-8 w-8">
                      <AvatarFallback className="text-xs">{userInfo.name.charAt(0)}</AvatarFallback>
                    </Avatar>
                  </Link>
                  <ThemeToggle />
                  <Button variant="ghost" size="sm" onClick={onLogout} title="Abmelden">
                    <LogOut className="h-4 w-4" />
                  </Button>
                </div>
              )}
            </div>
          </div>
        </aside>

        {/* Main content */}
        <main className={`flex-1 w-full transition-all duration-300 lg:min-h-screen ${
          sidebarCollapsed ? 'lg:ml-16' : 'lg:ml-64'
        }`}>
          <div className="py-6">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
              <Outlet />
            </div>
          </div>
        </main>
      </div>
    </div>
  );
};
