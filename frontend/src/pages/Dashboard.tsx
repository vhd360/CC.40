import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from '../components/ui/table';
import { Users, Zap, CreditCard, RefreshCw, Building2, Euro, Leaf, TrendingUp } from 'lucide-react';
import { api, DashboardStats, ChargingSession } from '../services/api';

export const Dashboard: React.FC = () => {
  const navigate = useNavigate();
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [subTenantCount, setSubTenantCount] = useState<number>(0);

  const loadData = async () => {
    try {
      // Load sub-tenants count
      const userStr = localStorage.getItem('user');
      
      let subTenantsCount = 0;
      if (userStr) {
        try {
          const tenants = await api.getTenants();
          const user = JSON.parse(userStr);
          // Filter out own tenant
          const filteredTenants = tenants.filter((tenant: any) => tenant.id !== user.tenantId);
          subTenantsCount = filteredTenants.length;
        } catch (err) {
          console.error('Failed to load sub-tenants:', err);
        }
      }
      
      setSubTenantCount(subTenantsCount);
      
      const [statsData, sessionsData] = await Promise.all([
        api.getDashboardStats(),
        api.getChargingSessions()
      ]);

      setStats(statsData);
    } catch (error) {
      console.error('Failed to load dashboard data:', error);
      // Fallback to mock data
      setStats({
        totalStations: 3,
        totalVehicles: 2,
        totalTransactions: 15,
        activeStations: 2,
        activeVehicles: 2
      });
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const statsCards = stats ? [
    {
      title: 'Gesamtumsatz',
      value: '€ 24,567',
      subtitle: '+12% zum Vormonat',
      icon: Euro,
      color: 'text-green-600',
      bgColor: 'bg-green-100'
    },
    {
      title: 'Verkaufte Energie',
      value: '3,842 kWh',
      subtitle: '+8% zum Vormonat',
      icon: Zap,
      color: 'text-blue-600',
      bgColor: 'bg-blue-100'
    },
    {
      title: 'CO₂ Einsparung',
      value: '2.1 Tonnen',
      subtitle: 'Diesen Monat',
      icon: Leaf,
      color: 'text-emerald-600',
      bgColor: 'bg-emerald-100'
    },
    {
      title: 'Sub-Tenants',
      value: subTenantCount.toString(),
      subtitle: 'Aktive Mandanten',
      icon: Building2,
      color: 'text-purple-600',
      bgColor: 'bg-purple-100'
    },
  ] : [];

  const refreshData = () => {
    loadData();
  };

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">Dashboard</h1>
          <p className="text-gray-600 dark:text-gray-400 mt-1">Business-Übersicht und Umsatz-Kennzahlen</p>
        </div>
        <Button onClick={refreshData} className="flex items-center space-x-2">
          <RefreshCw className="h-4 w-4" />
          <span>Aktualisieren</span>
        </Button>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {statsCards.map((stat) => (
          <Card key={stat.title} className="hover:shadow-lg transition-shadow">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium text-gray-600 dark:text-gray-400">
                {stat.title}
              </CardTitle>
              <div className={`w-10 h-10 ${stat.bgColor} dark:bg-opacity-20 rounded-lg flex items-center justify-center`}>
                <stat.icon className={`h-5 w-5 ${stat.color}`} />
              </div>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-gray-900 dark:text-gray-100">{stat.value}</div>
              <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                {stat.subtitle}
              </p>
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Main Content Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Revenue by Sub-Tenant */}
        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle>Umsatz nach Sub-Tenant</CardTitle>
            <CardDescription>
              Top-Performance Ihrer Sub-Tenants in diesem Monat
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Sub-Tenant</TableHead>
                  <TableHead>Verkaufte kWh</TableHead>
                  <TableHead>Ladevorgänge</TableHead>
                  <TableHead>CO₂ gespart</TableHead>
                  <TableHead className="text-right">Umsatz</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                <TableRow>
                  <TableCell className="font-medium">Langediers & Partner</TableCell>
                  <TableCell>1,245 kWh</TableCell>
                  <TableCell>87</TableCell>
                  <TableCell>
                    <span className="flex items-center text-green-600">
                      <Leaf className="h-4 w-4 mr-1" />
                      0.7 t
                    </span>
                  </TableCell>
                  <TableCell className="text-right font-medium text-green-600">
                    € 8,420
                  </TableCell>
                </TableRow>
                <TableRow>
                  <TableCell className="font-medium">Beispiel GmbH</TableCell>
                  <TableCell>987 kWh</TableCell>
                  <TableCell>65</TableCell>
                  <TableCell>
                    <span className="flex items-center text-green-600">
                      <Leaf className="h-4 w-4 mr-1" />
                      0.5 t
                    </span>
                  </TableCell>
                  <TableCell className="text-right font-medium text-green-600">
                    € 6,789
                  </TableCell>
                </TableRow>
                <TableRow>
                  <TableCell className="font-medium">Weitere Sub-Tenants</TableCell>
                  <TableCell>1,610 kWh</TableCell>
                  <TableCell>128</TableCell>
                  <TableCell>
                    <span className="flex items-center text-green-600">
                      <Leaf className="h-4 w-4 mr-1" />
                      0.9 t
                    </span>
                  </TableCell>
                  <TableCell className="text-right font-medium text-green-600">
                    € 9,358
                  </TableCell>
                </TableRow>
              </TableBody>
            </Table>
          </CardContent>
        </Card>

        {/* Monthly Trend */}
        <Card>
          <CardHeader>
            <CardTitle>Monatstrend</CardTitle>
            <CardDescription>
              Ihre wichtigsten KPIs im Überblick
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="p-4 bg-gradient-to-r from-green-50 to-emerald-50 dark:from-green-950 dark:to-emerald-950 rounded-lg border border-green-200 dark:border-green-800">
              <div className="flex items-center justify-between mb-2">
                <span className="text-sm font-medium text-gray-700 dark:text-gray-300">Umsatz</span>
                <TrendingUp className="h-4 w-4 text-green-600 dark:text-green-400" />
              </div>
              <div className="text-2xl font-bold text-gray-900 dark:text-gray-100">€ 24,567</div>
              <div className="text-xs text-green-600 dark:text-green-400 mt-1">+12% zum Vormonat</div>
            </div>

            <div className="p-4 bg-gradient-to-r from-blue-50 to-cyan-50 dark:from-blue-950 dark:to-cyan-950 rounded-lg border border-blue-200 dark:border-blue-800">
              <div className="flex items-center justify-between mb-2">
                <span className="text-sm font-medium text-gray-700 dark:text-gray-300">Energie</span>
                <Zap className="h-4 w-4 text-blue-600 dark:text-blue-400" />
              </div>
              <div className="text-2xl font-bold text-gray-900 dark:text-gray-100">3,842 kWh</div>
              <div className="text-xs text-blue-600 dark:text-blue-400 mt-1">+8% zum Vormonat</div>
            </div>

            <div className="p-4 bg-gradient-to-r from-emerald-50 to-teal-50 dark:from-emerald-950 dark:to-teal-950 rounded-lg border border-emerald-200 dark:border-emerald-800">
              <div className="flex items-center justify-between mb-2">
                <span className="text-sm font-medium text-gray-700 dark:text-gray-300">CO₂ Ersparnis</span>
                <Leaf className="h-4 w-4 text-emerald-600 dark:text-emerald-400" />
              </div>
              <div className="text-2xl font-bold text-gray-900 dark:text-gray-100">2.1 Tonnen</div>
              <div className="text-xs text-emerald-600 dark:text-emerald-400 mt-1">Entspricht 450 Bäumen</div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Quick Actions */}
      <Card>
        <CardHeader>
          <CardTitle>Schnellaktionen</CardTitle>
          <CardDescription>
            Häufig verwendete Funktionen
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <Button 
              className="flex flex-col items-center space-y-2 h-20 p-4" 
              variant="outline"
              onClick={() => navigate('/tenants')}
            >
              <Building2 className="h-6 w-6" />
              <span className="text-sm">Sub-Tenants</span>
            </Button>
            <Button 
              className="flex flex-col items-center space-y-2 h-20 p-4" 
              variant="outline"
              onClick={() => navigate('/billing')}
            >
              <CreditCard className="h-6 w-6" />
              <span className="text-sm">Abrechnung</span>
            </Button>
            <Button 
              className="flex flex-col items-center space-y-2 h-20 p-4" 
              variant="outline"
              onClick={() => navigate('/charging-stations')}
            >
              <Zap className="h-6 w-6" />
              <span className="text-sm">Ladestationen</span>
            </Button>
            <Button 
              className="flex flex-col items-center space-y-2 h-20 p-4" 
              variant="outline"
              onClick={() => navigate('/users')}
            >
              <Users className="h-6 w-6" />
              <span className="text-sm">Benutzer</span>
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
};
