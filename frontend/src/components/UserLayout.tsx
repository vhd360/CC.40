import React from 'react';
import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import { ThemeToggle } from './ThemeToggle';
import {
  LayoutDashboard,
  MapPin,
  Zap,
  CreditCard,
  Euro,
  Receipt,
  Car,
  LogOut,
  Menu,
  X,
  User,
  ChevronLeft,
  ChevronRight
} from 'lucide-react';

interface UserLayoutProps {
  onLogout: () => void;
}

export const UserLayout: React.FC<UserLayoutProps> = ({ onLogout }) => {
  const navigate = useNavigate();
  const location = useLocation();
  const [sidebarOpen, setSidebarOpen] = React.useState(false);
  const [sidebarCollapsed, setSidebarCollapsed] = React.useState(() => {
    const saved = localStorage.getItem('userSidebarCollapsed');
    return saved === 'true';
  });

  const userStr = localStorage.getItem('user');
  const user = userStr ? JSON.parse(userStr) : null;
  
  // Speichere Sidebar-Status
  React.useEffect(() => {
    localStorage.setItem('userSidebarCollapsed', sidebarCollapsed.toString());
  }, [sidebarCollapsed]);

  const navigation = [
    { name: 'Dashboard', href: '/user/dashboard', icon: LayoutDashboard },
    { name: 'Ladestationen', href: '/user/stations', icon: MapPin },
    { name: 'Ladevorgänge', href: '/user/sessions', icon: Zap },
    { name: 'Kosten', href: '/user/costs', icon: Euro },
    { name: 'Rechnungen', href: '/user/billing', icon: Receipt },
    { name: 'Meine Fahrzeuge', href: '/user/vehicles', icon: Car },
    { name: 'Identifikationsmethoden', href: '/user/auth-methods', icon: CreditCard },
  ];

  // Dev/Debug navigation (only show in development)
  const debugNavigation = process.env.NODE_ENV === 'development' ? [
    { name: '🔧 Debug', href: '/user/debug', icon: Menu }
  ] : [];

  const isActive = (path: string) => location.pathname === path;

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-950 overflow-x-hidden">
      {/* Mobile sidebar */}
      {sidebarOpen && (
        <div className="fixed inset-0 z-50 lg:hidden">
          <div className="fixed inset-0 bg-gray-600 bg-opacity-75" onClick={() => setSidebarOpen(false)} />
          <div className="fixed inset-y-0 left-0 flex w-64 flex-col bg-white dark:bg-gray-900">
            <div className="flex h-16 items-center justify-between px-4 border-b dark:border-gray-800">
              <h2 className="text-xl font-bold text-primary">CUBOS.Charge</h2>
              <button onClick={() => setSidebarOpen(false)} className="text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200">
                <X className="h-6 w-6" />
              </button>
            </div>
            <nav className="flex-1 overflow-y-auto space-y-1 px-2 py-4">
              {[...navigation, ...debugNavigation].map((item) => (
                <button
                  key={item.name}
                  onClick={() => {
                    navigate(item.href);
                    setSidebarOpen(false);
                  }}
                  className={`${
                    isActive(item.href)
                      ? 'bg-primary/10 text-primary'
                      : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-800'
                  } group flex w-full items-center px-3 py-2 text-sm font-medium rounded-md transition-colors`}
                >
                  <item.icon className="mr-3 h-5 w-5 flex-shrink-0" />
                  {item.name}
                </button>
              ))}
            </nav>
            
            {/* User info mobile */}
            {user && (
              <div className="flex-shrink-0 border-t border-gray-200 dark:border-gray-800 p-4">
                <div className="flex items-center justify-between mb-3">
                  <div className="flex items-center flex-1">
                    <div className="flex-shrink-0">
                      <div className="h-10 w-10 rounded-full bg-primary/10 flex items-center justify-center">
                        <User className="h-6 w-6 text-primary" />
                      </div>
                    </div>
                    <div className="ml-3 flex-1">
                      <p className="text-sm font-medium text-gray-900 dark:text-gray-100">
                        {user.firstName} {user.lastName}
                      </p>
                      <p className="text-xs text-gray-500 dark:text-gray-400">{user.email}</p>
                    </div>
                  </div>
                </div>
                <button
                  onClick={onLogout}
                  className="w-full flex items-center px-3 py-2 text-sm font-medium text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-950/50 rounded-md transition-colors"
                >
                  <LogOut className="mr-3 h-5 w-5" />
                  Abmelden
                </button>
              </div>
            )}
          </div>
        </div>
      )}

      {/* Desktop sidebar */}
      <div className={`hidden lg:fixed lg:inset-y-0 lg:flex lg:flex-col transition-all duration-300 overflow-hidden ${
        sidebarCollapsed ? 'lg:w-16' : 'lg:w-64'
      }`}>
        <div className="flex flex-col flex-grow bg-white dark:bg-gray-900 border-r border-gray-200 dark:border-gray-800 overflow-hidden">
          <div className="border-b dark:border-gray-800 p-4">
            {!sidebarCollapsed ? (
              <div className="flex items-center justify-between">
                <h1 className="text-xl font-bold text-primary truncate flex-1">CUBOS.Charge</h1>
                <button
                  onClick={() => setSidebarCollapsed(!sidebarCollapsed)}
                  className="text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200 flex-shrink-0 ml-2"
                >
                  <ChevronLeft className="h-5 w-5" />
                </button>
              </div>
            ) : (
              <div className="flex flex-col items-center space-y-3">
                <Zap className="h-8 w-8" style={{ color: 'var(--color-primary)' }} />
                <button
                  onClick={() => setSidebarCollapsed(!sidebarCollapsed)}
                  className="text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200 p-1"
                >
                  <ChevronRight className="h-5 w-5" />
                </button>
              </div>
            )}
          </div>
          <div className="flex-1 flex flex-col overflow-y-auto overflow-x-hidden">
            <nav className="flex-1 space-y-1 px-2 py-4 overflow-hidden">
              {[...navigation, ...debugNavigation].map((item) => (
                <button
                  key={item.name}
                  onClick={() => navigate(item.href)}
                  className={`${
                    isActive(item.href)
                      ? 'bg-primary/10 text-primary'
                      : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-800 hover:text-gray-900 dark:hover:text-gray-100'
                  } group relative flex w-full items-center px-3 py-2 text-sm font-medium rounded-md transition-colors ${
                    sidebarCollapsed ? 'justify-center' : ''
                  }`}
                  title={sidebarCollapsed ? item.name : undefined}
                >
                  <item.icon className={`h-5 w-5 flex-shrink-0 ${sidebarCollapsed ? '' : 'mr-3'}`} />
                  {!sidebarCollapsed && item.name}
                  
                  {/* Tooltip beim Hover im eingeklappten Zustand */}
                  {sidebarCollapsed && (
                    <div className="fixed left-20 px-3 py-2 bg-gray-900 dark:bg-gray-800 text-white text-sm rounded-md opacity-0 group-hover:opacity-100 pointer-events-none whitespace-nowrap z-[60] transition-opacity shadow-lg">
                      {item.name}
                      <div className="absolute left-0 top-1/2 -translate-x-1 -translate-y-1/2 w-2 h-2 bg-gray-900 dark:bg-gray-800 rotate-45"></div>
                    </div>
                  )}
                </button>
              ))}
            </nav>
          </div>

          {/* User info */}
          {user && (
            <div className="flex-shrink-0 border-t border-gray-200 dark:border-gray-800 p-4">
              {!sidebarCollapsed ? (
                <>
                  <div className="flex items-center justify-between mb-3">
                    <div className="flex items-center flex-1">
                      <div className="flex-shrink-0">
                        <div className="h-10 w-10 rounded-full bg-primary/10 flex items-center justify-center">
                          <User className="h-6 w-6 text-primary" />
                        </div>
                      </div>
                      <div className="ml-3 flex-1">
                        <p className="text-sm font-medium text-gray-900 dark:text-gray-100">
                          {user.firstName} {user.lastName}
                        </p>
                        <p className="text-xs text-gray-500 dark:text-gray-400">{user.email}</p>
                      </div>
                    </div>
                    <ThemeToggle />
                  </div>
                  <button
                    onClick={onLogout}
                    className="w-full flex items-center px-3 py-2 text-sm font-medium text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-950/50 rounded-md transition-colors"
                  >
                    <LogOut className="mr-3 h-5 w-5" />
                    Abmelden
                  </button>
                </>
              ) : (
                <div className="flex flex-col items-center space-y-3">
                  <div className="h-10 w-10 rounded-full bg-primary/10 flex items-center justify-center">
                    <User className="h-6 w-6 text-primary" />
                  </div>
                  <ThemeToggle />
                  <button
                    onClick={onLogout}
                    className="p-2 text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-950/50 rounded-md transition-colors"
                    title="Abmelden"
                  >
                    <LogOut className="h-5 w-5" />
                  </button>
                </div>
              )}
            </div>
          )}
        </div>
      </div>

      {/* Main content */}
      <div className={`flex flex-col flex-1 transition-all duration-300 overflow-x-hidden ${
        sidebarCollapsed ? 'lg:pl-16' : 'lg:pl-64'
      }`}>
        {/* Mobile header */}
        <div className="sticky top-0 z-10 lg:hidden flex h-16 items-center justify-between gap-x-4 border-b border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 px-4">
          <div className="flex items-center gap-x-4">
            <button onClick={() => setSidebarOpen(true)} className="text-gray-700 dark:text-gray-300">
              <Menu className="h-6 w-6" />
            </button>
            <h1 className="text-lg font-semibold text-primary">CUBOS.Charge</h1>
          </div>
          <ThemeToggle />
        </div>

        {/* Page content */}
        <main className="flex-1">
          <div className="py-6 px-4 sm:px-6 lg:px-8">
            <Outlet />
          </div>
        </main>
      </div>
    </div>
  );
};

