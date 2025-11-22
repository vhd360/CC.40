import React from 'react';
import { QRCodeSVG } from 'qrcode.react';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from './ui/dialog';
import { Button } from './ui/button';
import { Download, Printer } from 'lucide-react';

interface VehicleQRCodeDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  vehicle: {
    id: string;
    licensePlate: string;
    make: string;
    model: string;
    qrCode?: string;
  } | null;
}

export const VehicleQRCodeDialog: React.FC<VehicleQRCodeDialogProps> = ({ 
  open, 
  onOpenChange, 
  vehicle 
}) => {
  if (!vehicle) return null;

  // Generiere QR-Code-Daten (Fahrzeug-ID oder vorhandener QR-Code)
  const qrData = vehicle.qrCode || `VEHICLE-${vehicle.id}`;

  const handleDownload = () => {
    const svg = document.getElementById('vehicle-qr-code');
    if (!svg) return;

    const svgData = new XMLSerializer().serializeToString(svg);
    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d');
    const img = new Image();

    canvas.width = 300;
    canvas.height = 300;

    img.onload = () => {
      ctx?.drawImage(img, 0, 0);
      canvas.toBlob((blob) => {
        if (blob) {
          const url = URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = `QR-${vehicle.licensePlate}.png`;
          link.click();
          URL.revokeObjectURL(url);
        }
      });
    };

    img.src = 'data:image/svg+xml;base64,' + btoa(unescape(encodeURIComponent(svgData)));
  };

  const handlePrint = () => {
    const printWindow = window.open('', '', 'width=600,height=600');
    if (!printWindow) return;

    const svg = document.getElementById('vehicle-qr-code');
    if (!svg) return;

    printWindow.document.write(`
      <html>
        <head>
          <title>QR-Code - ${vehicle.licensePlate}</title>
          <style>
            body {
              display: flex;
              flex-direction: column;
              align-items: center;
              justify-content: center;
              min-height: 100vh;
              font-family: Arial, sans-serif;
              padding: 20px;
            }
            .qr-container {
              text-align: center;
              border: 2px solid #000;
              padding: 30px;
              border-radius: 10px;
            }
            h1 { font-size: 24px; margin-bottom: 10px; }
            h2 { font-size: 18px; color: #666; margin-bottom: 20px; }
            p { font-size: 14px; color: #888; margin-top: 20px; }
            @media print {
              body { margin: 0; }
              .no-print { display: none; }
            }
          </style>
        </head>
        <body>
          <div class="qr-container">
            <h1>${vehicle.make} ${vehicle.model}</h1>
            <h2>Kennzeichen: ${vehicle.licensePlate}</h2>
            ${svg.outerHTML}
            <p>QR-Code: ${qrData}</p>
          </div>
          <button class="no-print" onclick="window.print()" style="margin-top: 20px; padding: 10px 20px; font-size: 16px; cursor: pointer;">
            Drucken
          </button>
        </body>
      </html>
    `);
    printWindow.document.close();
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>QR-Code für Fahrzeug</DialogTitle>
          <DialogDescription>
            {vehicle.make} {vehicle.model} ({vehicle.licensePlate})
          </DialogDescription>
        </DialogHeader>

        <div className="flex flex-col items-center space-y-4 py-6">
          <div className="bg-white p-6 rounded-lg border-2 border-gray-200">
            <QRCodeSVG
              id="vehicle-qr-code"
              value={qrData}
              size={200}
              level="H"
              includeMargin={true}
            />
          </div>

          <div className="text-center space-y-2">
            <p className="text-sm font-medium text-gray-900 dark:text-gray-100">
              QR-Code Daten:
            </p>
            <code className="text-sm bg-gray-100 dark:bg-gray-800 px-3 py-1 rounded font-mono">
              {qrData}
            </code>
          </div>

          <div className="bg-blue-50 dark:bg-blue-950 border border-blue-200 dark:border-blue-800 rounded-lg p-4 w-full">
            <h4 className="font-semibold text-sm text-blue-900 dark:text-blue-100 mb-2">
              💡 Verwendung:
            </h4>
            <ul className="text-sm text-blue-800 dark:text-blue-200 space-y-1">
              <li>• QR-Code herunterladen oder ausdrucken</li>
              <li>• Am Fahrzeug anbringen (z.B. Armaturenbrett)</li>
              <li>• Benutzer scannen QR-Code zum Laden</li>
            </ul>
          </div>
        </div>

        <DialogFooter className="flex justify-between">
          <div className="flex gap-2">
            <Button variant="outline" onClick={handleDownload}>
              <Download className="h-4 w-4 mr-2" />
              Herunterladen
            </Button>
            <Button variant="outline" onClick={handlePrint}>
              <Printer className="h-4 w-4 mr-2" />
              Drucken
            </Button>
          </div>
          <Button variant="default" onClick={() => onOpenChange(false)}>
            Schließen
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};




