import React from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { QrCode as QrCodeIcon, Plus } from 'lucide-react';

export const QrCodes: React.FC = () => {
  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">QR-Code Verwaltung</h1>
          <p className="text-gray-600 dark:text-gray-400 mt-1">Generieren und verwalten Sie QR-Codes für Einladungen</p>
        </div>
        <Button className="flex items-center space-x-2">
          <Plus className="h-4 w-4" />
          <span>Neuer QR-Code</span>
        </Button>
      </div>

      <Card>
        <CardContent className="flex flex-col items-center justify-center py-12">
          <QrCodeIcon className="h-16 w-16 text-gray-300 mb-4" />
          <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-2">QR-Code Verwaltung</h3>
          <p className="text-gray-500 dark:text-gray-400 text-center max-w-md">
            Generieren Sie QR-Codes für Ladepark-Einladungen und Adhoc-Ladung.
            Diese Funktion wird bald verfügbar sein.
          </p>
        </CardContent>
      </Card>
    </div>
  );
};
