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
  UserCog
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
  const location = useLocation();
  
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
        <div className="fixed left-0 top-0 bottom-0 w-64 bg-white dark:bg-gray-900 shadow-lg">
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
          <nav className="p-4">
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
            </ul>
          </nav>
        </div>
      </div>

      {/* Header */}
      <header className="bg-white dark:bg-gray-900 shadow-sm border-b dark:border-gray-800 sticky top-0 z-40">
        <div className="w-full px-3 sm:px-4 lg:px-6">
          <div className="flex justify-between items-center py-4">
            <div className="flex items-center space-x-3 lg:space-x-4">
              <Button
                variant="ghost"
                size="sm"
                className="lg:hidden"
                onClick={() => setSidebarOpen(true)}
              >
                <Menu className="h-5 w-5" />
              </Button>
              <div className="flex items-center space-x-2 lg:space-x-3">
                {logoUrl ? (
                  <img src={`http://localhost:5126${logoUrl}`} alt={`${tenantName} Logo`} className="h-8 w-auto object-contain" />
                ) : (
                  <Zap className="h-8 w-8" style={{ color: 'var(--color-primary)' }} />
                )}
                <h1 className="text-xl font-bold text-gray-900 dark:text-gray-100">{tenantName}</h1>
              </div>
            </div>
            <div className="flex items-center space-x-4">
              <ThemeToggle />
              <Avatar>
                <AvatarFallback>{userInfo.name.charAt(0)}</AvatarFallback>
              </Avatar>
              <span className="text-sm font-medium text-gray-700 dark:text-gray-200">{userInfo.name}</span>
              <Button variant="outline" size="sm" onClick={onLogout}>
                Abmelden
              </Button>
            </div>
          </div>
        </div>
      </header>

      <div className="flex min-h-[calc(100vh-4rem)]">
        {/* Desktop sidebar */}
        <aside className="hidden lg:flex lg:w-64 lg:flex-col lg:fixed lg:left-0 lg:top-16 lg:bottom-0 lg:z-30">
          <div className="flex flex-col flex-grow bg-white dark:bg-gray-900 border-r border-gray-200 dark:border-gray-800 overflow-y-auto">
            <div className="flex flex-col flex-grow px-4 py-6">
              <nav className="flex-1 space-y-2">
                {navigation.map((item) => (
                  <Link
                    key={item.name}
                    to={item.href}
                    className={`flex items-center space-x-3 px-3 py-2 rounded-md text-sm font-medium transition-colors ${
                      location.pathname === item.href
                        ? 'bg-primary/10 text-primary hover:bg-primary/20'
                        : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-800 hover:text-gray-900 dark:hover:text-gray-100'
                    }`}
                  >
                    <item.icon className="h-4 w-4" />
                    <span>{item.name}</span>
                  </Link>
                ))}
              </nav>
            </div>
          </div>
        </aside>

        {/* Main content */}
        <main className="flex-1 w-full lg:ml-64">
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
