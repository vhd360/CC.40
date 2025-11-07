import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { Input } from '../components/ui/input';
import { Badge } from '../components/ui/badge';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from '../components/ui/table';
import { 
  CreditCard, 
  Euro, 
  TrendingUp, 
  Calendar, 
  Loader2,
  Download,
  RefreshCw,
  CheckCircle,
  XCircle,
  Clock,
  FileDown
} from 'lucide-react';
import { api, BillingTransaction, BillingSummary } from '../services/api';

export const Billing: React.FC = () => {
  const [loading, setLoading] = useState(true);
  const [transactions, setTransactions] = useState<BillingTransaction[]>([]);
  const [summary, setSummary] = useState<BillingSummary | null>(null);
  const [filteredTransactions, setFilteredTransactions] = useState<BillingTransaction[]>([]);
  
  // Filter states
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [searchQuery, setSearchQuery] = useState('');

  useEffect(() => {
    loadData();
  }, []);

  useEffect(() => {
    // Apply filters
    let filtered = transactions;

    if (statusFilter) {
      filtered = filtered.filter(t => t.status === statusFilter);
    }

    if (searchQuery) {
      const query = searchQuery.toLowerCase();
      filtered = filtered.filter(t => 
        t.description.toLowerCase().includes(query) ||
        t.account.accountName.toLowerCase().includes(query) ||
        (t.session?.user && t.session.user.toLowerCase().includes(query))
      );
    }

    setFilteredTransactions(filtered);
  }, [transactions, statusFilter, searchQuery]);

  const loadData = async () => {
    try {
      setLoading(true);
      const [transactionsData, summaryData] = await Promise.all([
        api.getBillingTransactions(),
        api.getBillingSummary()
      ]);
      setTransactions(transactionsData);
      setSummary(summaryData);
      setFilteredTransactions(transactionsData);
    } catch (error) {
      console.error('Failed to load billing data:', error);
    } finally {
      setLoading(false);
    }
  };

  const getStatusBadge = (status: string) => {
    const styles: Record<string, string> = {
      'Completed': 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400',
      'Pending': 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-400',
      'Failed': 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400',
      'Refunded': 'bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-300',
      'Cancelled': 'bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-300'
    };
    return styles[status] || 'bg-gray-100 text-gray-800';
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'Completed':
        return <CheckCircle className="h-4 w-4 text-green-600" />;
      case 'Pending':
        return <Clock className="h-4 w-4 text-yellow-600" />;
      case 'Failed':
      case 'Refunded':
      case 'Cancelled':
        return <XCircle className="h-4 w-4 text-red-600" />;
      default:
        return null;
    }
  };

  const formatCurrency = (amount: number, currency: string = 'EUR') => {
    return new Intl.NumberFormat('de-DE', {
      style: 'currency',
      currency: currency
    }).format(amount);
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
        <span className="ml-2 text-gray-600 dark:text-gray-400">Lade Abrechnungsdaten...</span>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">Abrechnung</h1>
          <p className="text-gray-600 dark:text-gray-400 mt-1">
            Verwaltung von Transaktionen und Zahlungen
          </p>
        </div>
        <div className="flex space-x-2">
          <Button variant="outline" onClick={loadData} className="flex items-center space-x-2">
            <RefreshCw className="h-4 w-4" />
            <span>Aktualisieren</span>
          </Button>
          <Button variant="outline" className="flex items-center space-x-2">
            <Download className="h-4 w-4" />
            <span>Exportieren</span>
          </Button>
        </div>
      </div>

      {/* Summary Cards */}
      {summary && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          <Card>
            <CardHeader className="pb-3">
              <div className="flex items-center justify-between">
                <CardTitle className="text-sm font-medium text-gray-600 dark:text-gray-400">
                  Gesamt-Umsatz
                </CardTitle>
                <Euro className="h-5 w-5 text-primary" />
              </div>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-gray-900 dark:text-gray-100">
                {formatCurrency(summary.totalRevenue, summary.currency)}
              </div>
              <p className="text-xs text-gray-600 dark:text-gray-400 mt-1">
                {summary.totalTransactions} Transaktionen
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-3">
              <div className="flex items-center justify-between">
                <CardTitle className="text-sm font-medium text-gray-600 dark:text-gray-400">
                  Diesen Monat
                </CardTitle>
                <Calendar className="h-5 w-5 text-primary" />
              </div>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-gray-900 dark:text-gray-100">
                {formatCurrency(summary.monthlyRevenue, summary.currency)}
              </div>
              <p className="text-xs text-gray-600 dark:text-gray-400 mt-1">
                {summary.monthlyTransactions} Transaktionen
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-3">
              <div className="flex items-center justify-between">
                <CardTitle className="text-sm font-medium text-gray-600 dark:text-gray-400">
                  Durchschnitt
                </CardTitle>
                <TrendingUp className="h-5 w-5 text-primary" />
              </div>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-gray-900 dark:text-gray-100">
                {formatCurrency(summary.averageTransactionValue, summary.currency)}
              </div>
              <p className="text-xs text-gray-600 dark:text-gray-400 mt-1">
                pro Transaktion
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-3">
              <div className="flex items-center justify-between">
                <CardTitle className="text-sm font-medium text-gray-600 dark:text-gray-400">
                  Status
                </CardTitle>
                <CreditCard className="h-5 w-5 text-primary" />
              </div>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-green-600 dark:text-green-400">
                {summary.completedTransactions}
              </div>
              <p className="text-xs text-gray-600 dark:text-gray-400 mt-1">
                {summary.pendingTransactions} ausstehend
              </p>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Filters */}
      <Card>
        <CardHeader>
          <CardTitle>Filter</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Suche
              </label>
              <Input
                placeholder="Beschreibung, Benutzer..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Status
              </label>
              <select
                value={statusFilter}
                onChange={(e) => setStatusFilter(e.target.value)}
                className="w-full rounded-md border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-900 px-3 py-2 text-sm"
              >
                <option value="">Alle Status</option>
                <option value="Completed">Abgeschlossen</option>
                <option value="Pending">Ausstehend</option>
                <option value="Failed">Fehlgeschlagen</option>
                <option value="Refunded">Erstattet</option>
              </select>
            </div>
            <div className="flex items-end">
              <Button 
                variant="outline" 
                onClick={() => {
                  setSearchQuery('');
                  setStatusFilter('');
                }}
                className="w-full"
              >
                Filter zurücksetzen
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Transactions Table */}
      <Card>
        <CardHeader>
          <CardTitle>Transaktionen</CardTitle>
          <CardDescription>
            {filteredTransactions.length} von {transactions.length} Transaktionen
          </CardDescription>
        </CardHeader>
        <CardContent>
          {filteredTransactions.length > 0 ? (
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Datum</TableHead>
                    <TableHead>Beschreibung</TableHead>
                    <TableHead>Benutzer/Account</TableHead>
                    <TableHead>Session</TableHead>
                    <TableHead className="text-right">Betrag</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Aktionen</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {filteredTransactions.map((transaction) => (
                    <TableRow key={transaction.id}>
                      <TableCell className="text-sm">
                        <div>
                          {new Date(transaction.createdAt).toLocaleDateString('de-DE', {
                            day: '2-digit',
                            month: '2-digit',
                            year: 'numeric'
                          })}
                        </div>
                        <div className="text-xs text-gray-500 dark:text-gray-400">
                          {new Date(transaction.createdAt).toLocaleTimeString('de-DE', {
                            hour: '2-digit',
                            minute: '2-digit'
                          })}
                        </div>
                      </TableCell>
                      <TableCell className="text-sm">
                        <div className="font-medium text-gray-900 dark:text-gray-100">
                          {transaction.description}
                        </div>
                        <div className="text-xs text-gray-500 dark:text-gray-400">
                          {transaction.transactionType}
                        </div>
                      </TableCell>
                      <TableCell className="text-sm">
                        <div className="font-medium text-gray-900 dark:text-gray-100">
                          {transaction.account.accountName}
                        </div>
                        {transaction.session && (
                          <div className="text-xs text-gray-500 dark:text-gray-400">
                            {transaction.session.user}
                          </div>
                        )}
                      </TableCell>
                      <TableCell className="text-sm">
                        {transaction.session ? (
                          <div>
                            <div className="text-gray-900 dark:text-gray-100">
                              {transaction.session.energyDelivered.toFixed(2)} kWh
                            </div>
                            <div className="text-xs text-gray-500 dark:text-gray-400">
                              {transaction.session.station}
                            </div>
                          </div>
                        ) : (
                          <span className="text-gray-400 dark:text-gray-600">-</span>
                        )}
                      </TableCell>
                      <TableCell className="text-right">
                        <div className="font-semibold text-gray-900 dark:text-gray-100">
                          {formatCurrency(transaction.amount, transaction.currency)}
                        </div>
                      </TableCell>
                      <TableCell>
                        <Badge className={getStatusBadge(transaction.status)}>
                          <div className="flex items-center space-x-1">
                            {getStatusIcon(transaction.status)}
                            <span>{transaction.status}</span>
                          </div>
                        </Badge>
                      </TableCell>
                      <TableCell>
                      <Button onClick={() => api.downloadInvoicePdf(transaction.id)}>
                        <FileDown className="h-4 w-4 mr-2" />
                        PDF Download
                      </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          ) : (
            <div className="text-center py-12">
              <CreditCard className="h-16 w-16 text-gray-300 dark:text-gray-700 mx-auto mb-4" />
              <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-2">
                Keine Transaktionen gefunden
              </h3>
              <p className="text-gray-600 dark:text-gray-400">
                {searchQuery || statusFilter
                  ? 'Versuchen Sie, die Filter anzupassen'
                  : 'Es gibt noch keine Transaktionen'}
              </p>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
};
