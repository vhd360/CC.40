import React, { useEffect, useRef, useState } from 'react';
import { Html5Qrcode } from 'html5-qrcode';
import { Button } from './ui/button';
import { Camera, X } from 'lucide-react';

interface QRScannerProps {
  onScan: (data: string) => void;
  onClose: () => void;
}

export const QRScanner: React.FC<QRScannerProps> = ({ onScan, onClose }) => {
  const scannerRef = useRef<Html5Qrcode | null>(null);
  const [isScanning, setIsScanning] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const startScanner = async () => {
      try {
        const scanner = new Html5Qrcode('qr-reader');
        scannerRef.current = scanner;

        await scanner.start(
          { facingMode: 'environment' }, // Rückkamera bevorzugen
          {
            fps: 10,
            qrbox: { width: 250, height: 250 }
          },
          (decodedText) => {
            console.log('QR-Code gescannt:', decodedText);
            onScan(decodedText);
            stopScanner();
          },
          (errorMessage) => {
            // Ignoriere Scan-Fehler (normal beim Scannen)
          }
        );

        setIsScanning(true);
      } catch (err: any) {
        console.error('Fehler beim Starten des Scanners:', err);
        setError(err.message || 'Kamera-Zugriff verweigert');
      }
    };

    startScanner();

    return () => {
      stopScanner();
    };
  }, []);

  const stopScanner = async () => {
    if (scannerRef.current && isScanning) {
      try {
        await scannerRef.current.stop();
        scannerRef.current.clear();
        setIsScanning(false);
      } catch (err) {
        console.error('Fehler beim Stoppen des Scanners:', err);
      }
    }
  };

  const handleClose = async () => {
    await stopScanner();
    onClose();
  };

  return (
    <div className="relative bg-gray-900 rounded-lg overflow-hidden">
      {error ? (
        <div className="p-8 text-center">
          <Camera className="h-16 w-16 text-red-500 mx-auto mb-4" />
          <h3 className="text-lg font-semibold text-white mb-2">Kamera-Zugriff erforderlich</h3>
          <p className="text-gray-400 mb-4">{error}</p>
          <p className="text-sm text-gray-500 mb-4">
            Bitte erlauben Sie den Kamera-Zugriff in Ihren Browser-Einstellungen.
          </p>
          <Button variant="outline" onClick={handleClose}>
            Schließen
          </Button>
        </div>
      ) : (
        <>
          <div className="relative">
            <div id="qr-reader" className="w-full" />
            <div className="absolute top-4 right-4">
              <Button
                variant="outline"
                size="icon"
                onClick={handleClose}
                className="bg-white/90 hover:bg-white"
              >
                <X className="h-4 w-4" />
              </Button>
            </div>
          </div>
          <div className="bg-gray-800 p-4 text-center">
            <p className="text-sm text-gray-300">
              🎯 Richten Sie die Kamera auf den QR-Code am Fahrzeug
            </p>
          </div>
        </>
      )}
    </div>
  );
};




