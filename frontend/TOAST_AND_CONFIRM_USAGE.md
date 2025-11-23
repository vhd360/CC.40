# Toast und ConfirmDialog - Verwendungsbeispiele

## Toast-System

### Verwendung

```typescript
import { useToast } from '../components/ui/toast';

const MyComponent = () => {
  const { showToast } = useToast();

  // Erfolg
  showToast('Operation erfolgreich!', 'success');
  
  // Fehler
  showToast('Ein Fehler ist aufgetreten', 'error');
  
  // Info
  showToast('Information', 'info');
  
  // Warnung
  showToast('Warnung: Bitte beachten Sie...', 'warning');
  
  // Mit benutzerdefinierter Dauer (in ms)
  showToast('Nachricht', 'success', 10000); // 10 Sekunden
};
```

### Toast-Typen

- `success` - Grüner Toast für Erfolgsmeldungen
- `error` - Roter Toast für Fehlermeldungen
- `info` - Blauer Toast für Informationen
- `warning` - Gelber Toast für Warnungen

## ConfirmDialog

### Verwendung

```typescript
import { useState } from 'react';
import { ConfirmDialog } from '../components/ConfirmDialog';

const MyComponent = () => {
  const [confirmDelete, setConfirmDelete] = useState<{
    open: boolean;
    itemId: string | null;
    itemName: string;
  }>({
    open: false,
    itemId: null,
    itemName: ''
  });

  const handleDeleteClick = (id: string, name: string) => {
    setConfirmDelete({
      open: true,
      itemId: id,
      itemName: name
    });
  };

  const handleDeleteConfirm = async () => {
    if (!confirmDelete.itemId) return;
    
    try {
      await api.deleteItem(confirmDelete.itemId);
      showToast('Erfolgreich gelöscht', 'success');
      // ... weitere Aktionen
    } catch (error: any) {
      showToast(error.message || 'Fehler beim Löschen', 'error');
    } finally {
      setConfirmDelete({ open: false, itemId: null, itemName: '' });
    }
  };

  return (
    <>
      <Button onClick={() => handleDeleteClick(item.id, item.name)}>
        Löschen
      </Button>

      <ConfirmDialog
        open={confirmDelete.open}
        onOpenChange={(open) => setConfirmDelete({ ...confirmDelete, open })}
        title="Element löschen"
        message={`Möchten Sie "${confirmDelete.itemName}" wirklich löschen? Diese Aktion kann nicht rückgängig gemacht werden.`}
        confirmText="Löschen"
        cancelText="Abbrechen"
        variant="destructive"
        onConfirm={handleDeleteConfirm}
      />
    </>
  );
};
```

### Props

- `open: boolean` - Ob das Dialog geöffnet ist
- `onOpenChange: (open: boolean) => void` - Callback wenn sich der Zustand ändert
- `title: string` - Titel des Dialogs
- `message: string` - Nachricht/Beschreibung
- `confirmText?: string` - Text für Bestätigungs-Button (Standard: "Bestätigen")
- `cancelText?: string` - Text für Abbrechen-Button (Standard: "Abbrechen")
- `variant?: 'default' | 'destructive'` - Variante (destructive zeigt Warn-Icon)
- `onConfirm: () => void` - Callback bei Bestätigung
- `onCancel?: () => void` - Optionaler Callback bei Abbrechen

## Migration von alert() und confirm()

### Vorher (mit alert/confirm):

```typescript
const handleDelete = async (id: string) => {
  if (!window.confirm('Wirklich löschen?')) return;
  
  try {
    await api.delete(id);
    alert('Erfolgreich gelöscht!');
  } catch (error) {
    alert('Fehler beim Löschen');
  }
};
```

### Nachher (mit Toast + ConfirmDialog):

```typescript
const [deleteConfirm, setDeleteConfirm] = useState({ open: false, id: null, name: '' });
const { showToast } = useToast();

const handleDeleteClick = (id: string, name: string) => {
  setDeleteConfirm({ open: true, id, name });
};

const handleDeleteConfirm = async () => {
  if (!deleteConfirm.id) return;
  
  try {
    await api.delete(deleteConfirm.id);
    showToast('Erfolgreich gelöscht!', 'success');
  } catch (error: any) {
    showToast(error.message || 'Fehler beim Löschen', 'error');
  } finally {
    setDeleteConfirm({ open: false, id: null, name: '' });
  }
};
```




## Toast-System

### Verwendung

```typescript
import { useToast } from '../components/ui/toast';

const MyComponent = () => {
  const { showToast } = useToast();

  // Erfolg
  showToast('Operation erfolgreich!', 'success');
  
  // Fehler
  showToast('Ein Fehler ist aufgetreten', 'error');
  
  // Info
  showToast('Information', 'info');
  
  // Warnung
  showToast('Warnung: Bitte beachten Sie...', 'warning');
  
  // Mit benutzerdefinierter Dauer (in ms)
  showToast('Nachricht', 'success', 10000); // 10 Sekunden
};
```

### Toast-Typen

- `success` - Grüner Toast für Erfolgsmeldungen
- `error` - Roter Toast für Fehlermeldungen
- `info` - Blauer Toast für Informationen
- `warning` - Gelber Toast für Warnungen

## ConfirmDialog

### Verwendung

```typescript
import { useState } from 'react';
import { ConfirmDialog } from '../components/ConfirmDialog';

const MyComponent = () => {
  const [confirmDelete, setConfirmDelete] = useState<{
    open: boolean;
    itemId: string | null;
    itemName: string;
  }>({
    open: false,
    itemId: null,
    itemName: ''
  });

  const handleDeleteClick = (id: string, name: string) => {
    setConfirmDelete({
      open: true,
      itemId: id,
      itemName: name
    });
  };

  const handleDeleteConfirm = async () => {
    if (!confirmDelete.itemId) return;
    
    try {
      await api.deleteItem(confirmDelete.itemId);
      showToast('Erfolgreich gelöscht', 'success');
      // ... weitere Aktionen
    } catch (error: any) {
      showToast(error.message || 'Fehler beim Löschen', 'error');
    } finally {
      setConfirmDelete({ open: false, itemId: null, itemName: '' });
    }
  };

  return (
    <>
      <Button onClick={() => handleDeleteClick(item.id, item.name)}>
        Löschen
      </Button>

      <ConfirmDialog
        open={confirmDelete.open}
        onOpenChange={(open) => setConfirmDelete({ ...confirmDelete, open })}
        title="Element löschen"
        message={`Möchten Sie "${confirmDelete.itemName}" wirklich löschen? Diese Aktion kann nicht rückgängig gemacht werden.`}
        confirmText="Löschen"
        cancelText="Abbrechen"
        variant="destructive"
        onConfirm={handleDeleteConfirm}
      />
    </>
  );
};
```

### Props

- `open: boolean` - Ob das Dialog geöffnet ist
- `onOpenChange: (open: boolean) => void` - Callback wenn sich der Zustand ändert
- `title: string` - Titel des Dialogs
- `message: string` - Nachricht/Beschreibung
- `confirmText?: string` - Text für Bestätigungs-Button (Standard: "Bestätigen")
- `cancelText?: string` - Text für Abbrechen-Button (Standard: "Abbrechen")
- `variant?: 'default' | 'destructive'` - Variante (destructive zeigt Warn-Icon)
- `onConfirm: () => void` - Callback bei Bestätigung
- `onCancel?: () => void` - Optionaler Callback bei Abbrechen

## Migration von alert() und confirm()

### Vorher (mit alert/confirm):

```typescript
const handleDelete = async (id: string) => {
  if (!window.confirm('Wirklich löschen?')) return;
  
  try {
    await api.delete(id);
    alert('Erfolgreich gelöscht!');
  } catch (error) {
    alert('Fehler beim Löschen');
  }
};
```

### Nachher (mit Toast + ConfirmDialog):

```typescript
const [deleteConfirm, setDeleteConfirm] = useState({ open: false, id: null, name: '' });
const { showToast } = useToast();

const handleDeleteClick = (id: string, name: string) => {
  setDeleteConfirm({ open: true, id, name });
};

const handleDeleteConfirm = async () => {
  if (!deleteConfirm.id) return;
  
  try {
    await api.delete(deleteConfirm.id);
    showToast('Erfolgreich gelöscht!', 'success');
  } catch (error: any) {
    showToast(error.message || 'Fehler beim Löschen', 'error');
  } finally {
    setDeleteConfirm({ open: false, id: null, name: '' });
  }
};
```


