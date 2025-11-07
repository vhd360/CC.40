import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Badge } from '../components/ui/badge';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from '../components/ui/table';
import { Loader2, Receipt, Euro, Calendar, Download, CheckCircle, Clock } from 'lucide-react';
import { api, BillingTransaction } from '../services/api';
import { Button } from '../components/ui/button';

export const UserBilling: React.FC = () => {
  const [loading, setLoading] = useState(true);
  const [transactions, setTransactions] = useState<BillingTransaction[]>([]);

  useEffect(() => {
    loadTransactions();
  }, []);

  const loadTransactions = async () => {
    try {
      setLoading(true);
      const data = await api.getUserBillingTransactions();
      setTransactions(data);
    } catch (error) {
      console.error('Failed to load billing transactions:', error);
    } finally {
      setLoading(false);
    }
  };

  const getStatusBadge = (status: string) => {
    const styles: Record<string, string> = {
      'Completed': 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400',
      'Pending': 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-400',
      'Failed': 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400'
    };
    return styles[status] || 'bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-300';
  };

  const formatCurrency = (amount: number, currency: string = 'EUR') => {
    return new Intl.NumberFormat('de-DE', {
      style: 'currency',
      currency: currency
    }).format(amount);
  };

  const totalAmount = transactions
    .filter(t => t.status === 'Completed')
    .reduce((sum, t) => sum + t.amount, 0);

  const currentMonth = new Date().getMonth();
  const currentYear = new Date().getFullYear();
  const monthlyAmount = transactions
    .filter(t => {
      const date = new Date(t.createdAt);
      return t.status === 'Completed' && 
             date.getMonth() === currentMonth && 
             date.getFullYear() === currentYear;
    })
    .reduce((sum, t) => sum + t.amount, 0);

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
        <span className="ml-2 text-gray-600 dark:text-gray-400">Lade Rechnungen...</span>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">Meine Rechnungen</h1>
          <p className="text-gray-600 dark:text-gray-400 mt-1">
            Übersicht Ihrer Ladevorgänge und Kosten
          </p>
        </div>
        <Button variant="outline" className="flex items-center space-x-2">
          <Download className="h-4 w-4" />
          <span>Exportieren</span>
        </Button>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card>
          <CardHeader className="pb-3">
            <div className="flex items-center justify-between">
              <CardTitle className="text-sm font-medium text-gray-600 dark:text-gray-400">
                Gesamt-Kosten
              </CardTitle>
              <Euro className="h-5 w-5 text-primary" />
            </div>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-gray-900 dark:text-gray-100">
              {formatCurrency(totalAmount)}
            </div>
            <p className="text-xs text-gray-600 dark:text-gray-400 mt-1">
              {transactions.filter(t => t.status === 'Completed').length} Transaktionen
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
              {formatCurrency(monthlyAmount)}
            </div>
            <p className="text-xs text-gray-600 dark:text-gray-400 mt-1">
              {transactions.filter(t => {
                const date = new Date(t.createdAt);
                return date.getMonth() === currentMonth && date.getFullYear() === currentYear;
              }).length} Transaktionen
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-3">
            <div className="flex items-center justify-between">
              <CardTitle className="text-sm font-medium text-gray-600 dark:text-gray-400">
                Status
              </CardTitle>
              <Receipt className="h-5 w-5 text-primary" />
            </div>
          </CardHeader>
          <CardContent>
            <div className="flex items-center space-x-2">
              <CheckCircle className="h-6 w-6 text-green-600 dark:text-green-400" />
              <div className="text-2xl font-bold text-green-600 dark:text-green-400">
                {transactions.filter(t => t.status === 'Completed').length}
              </div>
            </div>
            <p className="text-xs text-gray-600 dark:text-gray-400 mt-1">
              {transactions.filter(t => t.status === 'Pending').length} ausstehend
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Transactions Table */}
      <Card>
        <CardHeader>
          <CardTitle>Alle Transaktionen</CardTitle>
          <CardDescription>
            Ihre Ladekosten im Detail
          </CardDescription>
        </CardHeader>
        <CardContent>
          {transactions.length > 0 ? (
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Datum</TableHead>
                    <TableHead>Ladestation</TableHead>
                    <TableHead>Energie</TableHead>
                    <TableHead className="text-right">Betrag</TableHead>
                    <TableHead>Status</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {transactions.map((transaction) => (
                    <TableRow key={transaction.id}>
                      <TableCell className="text-sm">
                        <div className="font-medium text-gray-900 dark:text-gray-100">
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
                        {transaction.session ? (
                          <div>
                            <div className="font-medium text-gray-900 dark:text-gray-100">
                              {transaction.session.station}
                            </div>
                            <div className="text-xs text-gray-500 dark:text-gray-400">
                              Session #{transaction.session.sessionId}
                            </div>
                          </div>
                        ) : (
                          <span className="text-gray-400 dark:text-gray-600">-</span>
                        )}
                      </TableCell>
                      <TableCell className="text-sm">
                        {transaction.session ? (
                          <div className="font-medium text-gray-900 dark:text-gray-100">
                            {transaction.session.energyDelivered.toFixed(2)} kWh
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
                            {transaction.status === 'Completed' ? (
                              <CheckCircle className="h-3 w-3" />
                            ) : (
                              <Clock className="h-3 w-3" />
                            )}
                            <span>{transaction.status}</span>
                          </div>
                        </Badge>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          ) : (
            <div className="text-center py-12">
              <Receipt className="h-16 w-16 text-gray-300 dark:text-gray-700 mx-auto mb-4" />
              <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-2">
                Noch keine Rechnungen
              </h3>
              <p className="text-gray-600 dark:text-gray-400">
                Ihre Ladekosten werden hier angezeigt
              </p>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
};

