import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from '../components/ui/table';
import { Loader2, Euro, TrendingUp, Calendar, Building2 } from 'lucide-react';
import { api } from '../services/api';

export const UserCosts: React.FC = () => {
  const [loading, setLoading] = useState(true);
  const [costsData, setCostsData] = useState<any>(null);
  const [selectedYear, setSelectedYear] = useState(new Date().getFullYear());

  useEffect(() => {
    loadCosts();
  }, [selectedYear]);

  const loadCosts = async () => {
    try {
      setLoading(true);
      const data = await api.getUserCosts(selectedYear);
      setCostsData(data);
    } catch (error) {
      console.error('Failed to load costs:', error);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
        <span className="ml-2 text-gray-600">Lade Kostendaten...</span>
      </div>
    );
  }

  if (!costsData) {
    return (
      <div className="text-center py-12">
        <p className="text-gray-600">Fehler beim Laden der Kostendaten</p>
      </div>
    );
  }

  const monthNames = [
    'Januar', 'Februar', 'März', 'April', 'Mai', 'Juni',
    'Juli', 'August', 'September', 'Oktober', 'November', 'Dezember'
  ];

  const totalYearCost = costsData.monthlyCosts.reduce((sum: number, m: any) => sum + m.totalCost, 0);
  const totalYearEnergy = costsData.monthlyCosts.reduce((sum: number, m: any) => sum + m.totalEnergy, 0);
  const totalYearSessions = costsData.monthlyCosts.reduce((sum: number, m: any) => sum + m.sessionCount, 0);
  const avgCostPerKwh = totalYearEnergy > 0 ? totalYearCost / totalYearEnergy : 0;

  // Prepare chart data (last 12 months with zeros for missing months)
  const chartData = monthNames.map((name, index) => {
    const monthData = costsData.monthlyCosts.find((m: any) => m.month === index + 1);
    return {
      month: name.substring(0, 3),
      cost: monthData ? monthData.totalCost : 0,
      energy: monthData ? monthData.totalEnergy : 0
    };
  });

  const maxCost = Math.max(...chartData.map(d => d.cost), 1);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Kosten & Abrechnung</h1>
          <p className="text-gray-600 mt-1">Übersicht Ihrer Ladekosten</p>
        </div>
        <div className="flex items-center space-x-2">
          <label className="text-sm text-gray-600">Jahr:</label>
          <select
            value={selectedYear}
            onChange={(e) => setSelectedYear(parseInt(e.target.value))}
            className="border rounded-md px-3 py-2 text-sm"
          >
            {[2024, 2023, 2022].map(year => (
              <option key={year} value={year}>{year}</option>
            ))}
          </select>
        </div>
      </div>

      {/* Summary Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-gray-600">Gesamtkosten {selectedYear}</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-orange-600">€{totalYearCost.toFixed(2)}</div>
            <p className="text-xs text-gray-500">{totalYearSessions} Ladevorgänge</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-gray-600">Energie {selectedYear}</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-green-600">{totalYearEnergy.toFixed(1)} kWh</div>
            <p className="text-xs text-gray-500">Geladen</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-gray-600">Ø Kosten/kWh</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">€{avgCostPerKwh.toFixed(3)}</div>
            <p className="text-xs text-gray-500">Durchschnitt</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-gray-600">Aktueller Monat</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-blue-600">
              €{(costsData.monthlyCosts.find((m: any) => m.month === costsData.month)?.totalCost || 0).toFixed(2)}
            </div>
            <p className="text-xs text-gray-500">{monthNames[costsData.month - 1]}</p>
          </CardContent>
        </Card>
      </div>

      {/* Cost Chart */}
      <Card>
        <CardHeader>
          <CardTitle>Monatliche Kosten {selectedYear}</CardTitle>
          <CardDescription>Übersicht der monatlichen Ausgaben</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-3">
            {chartData.map((data, index) => (
              <div key={index} className="space-y-1">
                <div className="flex items-center justify-between text-sm">
                  <span className="w-12 text-gray-600">{data.month}</span>
                  <div className="flex-1 mx-4">
                    <div className="h-8 bg-gray-100 rounded-full overflow-hidden">
                      <div
                        className="h-full bg-gradient-to-r from-orange-400 to-orange-600 rounded-full transition-all flex items-center justify-end pr-2"
                        style={{ width: `${(data.cost / maxCost) * 100}%` }}
                      >
                        {data.cost > 0 && (
                          <span className="text-white text-xs font-medium">
                            €{data.cost.toFixed(2)}
                          </span>
                        )}
                      </div>
                    </div>
                  </div>
                  <span className="w-24 text-right text-gray-500 text-xs">
                    {data.energy.toFixed(1)} kWh
                  </span>
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Monthly Breakdown */}
      <Card>
        <CardHeader>
          <CardTitle>Monatliche Aufschlüsselung</CardTitle>
          <CardDescription>Detaillierte Übersicht pro Monat</CardDescription>
        </CardHeader>
        <CardContent>
          {costsData.monthlyCosts.length > 0 ? (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Monat</TableHead>
                  <TableHead>Ladevorgänge</TableHead>
                  <TableHead>Energie</TableHead>
                  <TableHead>Kosten</TableHead>
                  <TableHead>Ø €/kWh</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {costsData.monthlyCosts.map((month: any) => (
                  <TableRow key={month.month}>
                    <TableCell className="font-medium">
                      <div className="flex items-center space-x-2">
                        <Calendar className="h-4 w-4 text-gray-400" />
                        <span>{monthNames[month.month - 1]}</span>
                      </div>
                    </TableCell>
                    <TableCell>{month.sessionCount}</TableCell>
                    <TableCell className="font-medium">
                      {month.totalEnergy.toFixed(1)} kWh
                    </TableCell>
                    <TableCell className="font-medium text-orange-600">
                      €{month.totalCost.toFixed(2)}
                    </TableCell>
                    <TableCell className="text-gray-600">
                      €{month.totalEnergy > 0 ? (month.totalCost / month.totalEnergy).toFixed(3) : '0.000'}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          ) : (
            <div className="text-center py-8 text-gray-500">
              <Euro className="h-12 w-12 mx-auto mb-3 opacity-30" />
              <p>Keine Kostendaten für {selectedYear}</p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Costs by Charging Park */}
      <Card>
        <CardHeader>
          <CardTitle>Kosten nach Ladepark</CardTitle>
          <CardDescription>Wo haben Sie am meisten geladen?</CardDescription>
        </CardHeader>
        <CardContent>
          {costsData.costsByPark.length > 0 ? (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Ladepark</TableHead>
                  <TableHead>Ladevorgänge</TableHead>
                  <TableHead>Gesamtkosten</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {costsData.costsByPark.map((park: any) => (
                  <TableRow key={park.parkId}>
                    <TableCell className="font-medium">
                      <div className="flex items-center space-x-2">
                        <Building2 className="h-4 w-4 text-gray-400" />
                        <span>{park.parkName}</span>
                      </div>
                    </TableCell>
                    <TableCell>{park.sessionCount}</TableCell>
                    <TableCell className="font-medium text-orange-600">
                      €{park.totalCost.toFixed(2)}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          ) : (
            <div className="text-center py-8 text-gray-500">
              <Building2 className="h-12 w-12 mx-auto mb-3 opacity-30" />
              <p>Keine Daten verfügbar</p>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
};


