const API_BASE_URL = 'http://localhost:5126/api';

// Helper function to get auth headers
const getAuthHeaders = (): HeadersInit => {
  const token = localStorage.getItem('token');
  return {
    'Content-Type': 'application/json',
    ...(token && { 'Authorization': `Bearer ${token}` })
  };
};

// API Response types
export interface Tenant {
  id: string;
  name: string;
  subdomain: string;
  description?: string;
  isActive: boolean;
  createdAt: string;
  userCount: number;
}

export interface ChargingStation {
  id: string;
  stationId: string;
  name: string;
  status: string;
  latitude?: number;
  longitude?: number;
  maxPower: number;
  numberOfConnectors: number;
  vendor: string;
  model: string;
  type: string;
  createdAt: string;
  lastHeartbeat?: string;
}

export interface Vehicle {
  id: string;
  licensePlate: string;
  make: string;
  model: string;
  year: number;
  type: string;
  color: string;
  notes?: string;
  rfidTag?: string;
  qrCode?: string;
  isActive: boolean;
  createdAt: string;
}

export interface ChargingSession {
  id: string;
  user?: string;
  vehicle: string;
  station: string;
  duration: string;
  cost: string;
  status: string;
  startedAt: string;
}

export interface DashboardStats {
  totalStations: number;
  totalVehicles: number;
  totalTransactions: number;
  activeStations: number;
  activeVehicles: number;
}

export interface Permission {
  id: string;
  name: string;
  resource: string;
  action: string;
  description?: string;
}

export interface GroupPermission {
  id: string;
  permissionId: string;
  permission: Permission;
  assignedAt: string;
}

export interface Tariff {
  id: string;
  name: string;
  description?: string;
  currency: string;
  isDefault: boolean;
  isActive: boolean;
  validFrom?: string;
  validUntil?: string;
  createdAt: string;
  updatedAt?: string;
  components: TariffComponent[];
  userGroups: UserGroupTariff[];
}

export interface TariffComponent {
  id: string;
  type: TariffComponentType;
  price: number;
  stepSize?: number;
  timeStart?: string;
  timeEnd?: string;
  daysOfWeek?: string;
  minimumCharge?: number;
  maximumCharge?: number;
  gracePeriodMinutes?: number;
  displayOrder: number;
  isActive: boolean;
}

export interface UserGroupTariff {
  userGroupId: string;
  userGroupName: string;
  priority: number;
}

export enum TariffComponentType {
  Energy = 0,
  ChargingTime = 1,
  ParkingTime = 2,
  SessionFee = 3,
  IdleTime = 4,
  TimeOfDay = 5
}

export interface CreateTariffRequest {
  name: string;
  description?: string;
  currency?: string;
  isDefault: boolean;
  isActive: boolean;
  validFrom?: string;
  validUntil?: string;
  components: TariffComponentRequest[];
}

export interface TariffComponentRequest {
  type: TariffComponentType;
  price: number;
  stepSize?: number;
  timeStart?: string;
  timeEnd?: string;
  daysOfWeek?: string;
  minimumCharge?: number;
  maximumCharge?: number;
  gracePeriodMinutes?: number;
  displayOrder: number;
}

export interface SessionCostBreakdown {
  sessionId: string;
  sessionNumber: string;
  ocppTransactionId?: number;
  startedAt: string;
  endedAt?: string;
  durationMinutes: number;
  durationFormatted: string;
  energyDelivered: number;
  energyDeliveredFormatted: string;
  totalCost: number;
  currency: string;
  totalCostFormatted: string;
  costBreakdown: CostBreakdownItem[];
  appliedTariff?: {
    id: string;
    name: string;
    description?: string;
    currency: string;
  };
  station: {
    id: string;
    name: string;
    stationId: string;
    chargingPark: {
      name: string;
      address?: string;
      city?: string;
      postalCode?: string;
    };
  };
  connector: {
    id: string;
    connectorId: number;
    type: string;
  };
  vehicle?: {
    id: string;
    make: string;
    model: string;
    licensePlate: string;
  };
  authorizationMethod?: {
    id: string;
    type: string;
    friendlyName: string;
    identifier: string;
  };
  status: string;
}

export interface CostBreakdownItem {
  component: string;
  cost: number;
  costFormatted: string;
}

export interface BillingTransaction {
  id: string;
  amount: number;
  currency: string;
  description: string;
  status: string;
  transactionType: string;
  createdAt: string;
  processedAt?: string;
  account: {
    id: string;
    accountName: string;
  };
  session?: {
    id: string;
    sessionId: string;
    energyDelivered: number;
    user: string;
    station: string;
  };
}

export interface BillingSummary {
  totalRevenue: number;
  totalTransactions: number;
  completedTransactions: number;
  pendingTransactions: number;
  monthlyRevenue: number;
  monthlyTransactions: number;
  yearlyRevenue: number;
  yearlyTransactions: number;
  averageTransactionValue: number;
  currency: string;
}

export interface BillingAccount {
  id: string;
  accountName: string;
  type: string;
  status: string;
  createdAt: string;
  transactionCount: number;
  totalAmount: number;
}

export interface VehicleAssignment {
  id: string;
  vehicleId: string;
  vehicle: {
    id: string;
    licensePlate: string;
    make: string;
    model: string;
    year?: number;
    color?: string;
    type: string;
    rfidTag?: string;
    qrCode?: string;
  };
  userId: string;
  user: {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
    phoneNumber?: string;
  };
  assignmentType: string; // "Permanent", "Temporary", "Reservation"
  assignedAt: string;
  returnedAt?: string;
  notes?: string;
  isActive: boolean;
}

export interface CreateVehicleAssignmentRequest {
  vehicleId: string;
  userId: string;
  assignmentType: string; // "Permanent", "Temporary", "Reservation"
  notes?: string;
}

export interface UpdateVehicleAssignmentRequest {
  assignmentType?: string;
  notes?: string;
}

// API Functions
export const api = {
  // Tenants
  async getTenants(): Promise<Tenant[]> {
    const response = await fetch(`${API_BASE_URL}/tenants`, {
      headers: getAuthHeaders()
    });
    if (!response.ok) {
      // Fallback to mock data if API is not available
      return getMockTenants();
    }
    return response.json();
  },

  async createTenant(tenant: Omit<Tenant, 'id' | 'createdAt' | 'userCount'>): Promise<Tenant> {
    const response = await fetch(`${API_BASE_URL}/tenants`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(tenant),
    });
    if (!response.ok) throw new Error('Failed to create tenant');
    return response.json();
  },

  async updateTenant(id: string, tenant: Partial<Tenant>): Promise<Tenant> {
    const response = await fetch(`${API_BASE_URL}/tenants/${id}`, {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify(tenant),
    });
    if (!response.ok) throw new Error('Failed to update tenant');
    return response.json();
  },

  async deleteTenant(id: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/tenants/${id}`, {
      method: 'DELETE',
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to delete tenant');
  },

  async getTenantById(id: string): Promise<Tenant> {
    const response = await fetch(`${API_BASE_URL}/tenants/${id}`, {
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to fetch tenant');
    return response.json();
  },

  // Users
  async getUsers(tenantId?: string): Promise<any[]> {
    const url = tenantId ? `${API_BASE_URL}/users?tenantId=${tenantId}` : `${API_BASE_URL}/users`;
    const response = await fetch(url, {
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to fetch users');
    return response.json();
  },

  async createUser(user: { 
    tenantId: string;
    firstName: string; 
    lastName: string; 
    email: string; 
    phoneNumber?: string; 
    password: string;
    role: string;
  }): Promise<any> {
    // Convert to PascalCase for C# backend and parse role as integer
    const dto = {
      TenantId: user.tenantId,
      FirstName: user.firstName,
      LastName: user.lastName,
      Email: user.email,
      PhoneNumber: user.phoneNumber || null,
      Password: user.password,
      Role: parseInt(user.role) // Parse role as integer (0=User, 1=TenantAdmin)
    };

    const response = await fetch(`${API_BASE_URL}/users`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(dto),
    });
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || 'Failed to create user');
    }
    return response.json();
  },

  async updateUser(id: string, user: any): Promise<any> {
    const response = await fetch(`${API_BASE_URL}/users/${id}`, {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify(user),
    });
    if (!response.ok) throw new Error('Failed to update user');
    return response.json();
  },

  async deleteUser(id: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/users/${id}`, {
      method: 'DELETE',
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to delete user');
  },

  async removeGuestUserFromTenant(userId: string): Promise<any> {
    const response = await fetch(`${API_BASE_URL}/users/${userId}/remove-from-tenant`, {
      method: 'DELETE',
      headers: getAuthHeaders(),
    });
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || 'Failed to remove guest user');
    }
    return response.json();
  },

  // Charging Parks
  async getChargingParks(): Promise<any[]> {
    const response = await fetch(`${API_BASE_URL}/charging-parks`, {
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to fetch charging parks');
    return response.json();
  },

  async createChargingPark(park: any): Promise<any> {
    const response = await fetch(`${API_BASE_URL}/charging-parks`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(park),
    });
    if (!response.ok) throw new Error('Failed to create charging park');
    return response.json();
  },

  async updateChargingPark(id: string, park: any): Promise<any> {
    const response = await fetch(`${API_BASE_URL}/charging-parks/${id}`, {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify(park),
    });
    if (!response.ok) throw new Error('Failed to update charging park');
    return response.json();
  },

  async deleteChargingPark(id: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/charging-parks/${id}`, {
      method: 'DELETE',
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to delete charging park');
  },

  // Charging Stations
  async getChargingStations(): Promise<ChargingStation[]> {
    try {
      const response = await fetch(`${API_BASE_URL}/charging-stations`, {
        headers: getAuthHeaders()
      });
      if (!response.ok) {
        return getMockChargingStations();
      }
      return response.json();
    } catch {
      return getMockChargingStations();
    }
  },

  async createChargingStation(station: {
    chargingParkId: string;
    stationId: string;
    name: string;
    vendor: string;
    model: string;
    type: number;
    maxPower: number;
    numberOfConnectors: number;
    latitude?: number;
    longitude?: number;
    notes?: string;
    chargeBoxId?: string;
    ocppPassword?: string;
    ocppProtocol?: string;
    ocppEndpoint?: string;
  }): Promise<any> {
    const dto = {
      ChargingParkId: station.chargingParkId,
      StationId: station.stationId,
      Name: station.name,
      Vendor: station.vendor,
      Model: station.model,
      Type: station.type,
      MaxPower: station.maxPower,
      NumberOfConnectors: station.numberOfConnectors,
      Latitude: station.latitude || null,
      Longitude: station.longitude || null,
      Notes: station.notes || null,
      ChargeBoxId: station.chargeBoxId || null,
      OcppPassword: station.ocppPassword || null,
      OcppProtocol: station.ocppProtocol || null,
      OcppEndpoint: station.ocppEndpoint || null
    };

    const response = await fetch(`${API_BASE_URL}/charging-stations`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(dto)
    });
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || 'Failed to create charging station');
    }
    return response.json();
  },

  // Vehicles
  async getVehicles(): Promise<Vehicle[]> {
    try {
      const response = await fetch(`${API_BASE_URL}/vehicles`, {
        headers: getAuthHeaders()
      });
      if (!response.ok) {
        return getMockVehicles();
      }
      return response.json();
    } catch {
      return getMockVehicles();
    }
  },

  async createVehicle(vehicle: Omit<Vehicle, 'id' | 'createdAt' | 'isActive'>): Promise<Vehicle> {
    const response = await fetch(`${API_BASE_URL}/vehicles`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(vehicle),
    });
    if (!response.ok) throw new Error('Failed to create vehicle');
    return response.json();
  },

  async updateVehicle(id: string, vehicle: Partial<Vehicle>): Promise<Vehicle> {
    const response = await fetch(`${API_BASE_URL}/vehicles/${id}`, {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify(vehicle),
    });
    if (!response.ok) throw new Error('Failed to update vehicle');
    return response.json();
  },

  async deleteVehicle(id: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/vehicles/${id}`, {
      method: 'DELETE',
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to delete vehicle');
  },

  // Charging Sessions
  async getChargingSessions(): Promise<ChargingSession[]> {
    try {
      const response = await fetch(`${API_BASE_URL}/charging/sessions`, {
        headers: getAuthHeaders()
      });
      if (!response.ok) {
        return getMockChargingSessions();
      }
      return response.json();
    } catch {
      return getMockChargingSessions();
    }
  },

  // Billing
  async getBillingTransactions(): Promise<BillingTransaction[]> {
    try {
      const response = await fetch(`${API_BASE_URL}/billing/transactions`, {
        headers: getAuthHeaders()
      });
      if (!response.ok) {
        return getMockBillingTransactions();
      }
      return response.json();
    } catch {
      return getMockBillingTransactions();
    }
  },

  // Dashboard Stats
  async getDashboardStats(): Promise<DashboardStats> {
    try {
      const response = await fetch(`${API_BASE_URL}/dashboard/stats`, {
        headers: getAuthHeaders()
      });
      if (!response.ok) {
        return getMockDashboardStats();
      }
      return response.json();
    } catch {
      return getMockDashboardStats();
    }
  },

  // User Groups
  async getUserGroups(): Promise<any[]> {
    const response = await fetch(`${API_BASE_URL}/user-groups`, {
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to fetch user groups');
    return response.json();
  },

  async createUserGroup(group: { name: string; description?: string }): Promise<any> {
    const response = await fetch(`${API_BASE_URL}/user-groups`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(group),
    });
    if (!response.ok) throw new Error('Failed to create user group');
    return response.json();
  },

  async updateUserGroup(id: string, group: any): Promise<any> {
    const response = await fetch(`${API_BASE_URL}/user-groups/${id}`, {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify(group),
    });
    if (!response.ok) throw new Error('Failed to update user group');
    return response.json();
  },

  async deleteUserGroup(id: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/user-groups/${id}`, {
      method: 'DELETE',
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to delete user group');
  },

  async getUserGroupDetails(id: string): Promise<any> {
    const response = await fetch(`${API_BASE_URL}/user-groups/${id}`, {
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to fetch user group details');
    return response.json();
  },

  async addUserToGroup(groupId: string, userId: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/user-groups/${groupId}/users/${userId}`, {
      method: 'POST',
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to add user to group');
  },

  async removeUserFromGroup(groupId: string, userId: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/user-groups/${groupId}/users/${userId}`, {
      method: 'DELETE',
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to remove user from group');
  },

  async generateGroupInvite(groupId: string, expiryDays: number = 7): Promise<any> {
    const response = await fetch(`${API_BASE_URL}/user-groups/${groupId}/generate-invite`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify({ expiryDays })
    });
    if (!response.ok) throw new Error('Failed to generate invite');
    return response.json();
  },

  async joinGroupWithToken(token: string): Promise<any> {
    const response = await fetch(`${API_BASE_URL}/user-groups/join`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify({ token })
    });
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Failed to join group');
    }
    return response.json();
  },

  async revokeGroupInvite(groupId: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/user-groups/${groupId}/revoke-invite`, {
      method: 'DELETE',
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to revoke invite');
  },

  // Permissions
  async getAllPermissions(): Promise<Permission[]> {
    const response = await fetch(`${API_BASE_URL}/permissions`, {
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to fetch permissions');
    return response.json();
  },

  async getUserGroupPermissions(groupId: string): Promise<GroupPermission[]> {
    const response = await fetch(`${API_BASE_URL}/permissions/user-group/${groupId}`, {
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to fetch group permissions');
    return response.json();
  },

  async addPermissionToGroup(groupId: string, permissionId: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/permissions/user-group/${groupId}/permissions/${permissionId}`, {
      method: 'POST',
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to add permission to group');
  },

  async removePermissionFromGroup(groupId: string, permissionId: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/permissions/user-group/${groupId}/permissions/${permissionId}`, {
      method: 'DELETE',
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to remove permission from group');
  },

  async setGroupPermissions(groupId: string, permissionIds: string[]): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/permissions/user-group/${groupId}/permissions`, {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify({ permissionIds })
    });
    if (!response.ok) throw new Error('Failed to set group permissions');
  },

  // Charging Station Groups
  async getChargingStationGroups(): Promise<any[]> {
    const response = await fetch(`${API_BASE_URL}/charging-station-groups`, {
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to fetch charging station groups');
    return response.json();
  },

  async createChargingStationGroup(group: { name: string; description?: string }): Promise<any> {
    const response = await fetch(`${API_BASE_URL}/charging-station-groups`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(group),
    });
    if (!response.ok) throw new Error('Failed to create charging station group');
    return response.json();
  },

  async updateChargingStationGroup(id: string, group: any): Promise<any> {
    const response = await fetch(`${API_BASE_URL}/charging-station-groups/${id}`, {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify(group),
    });
    if (!response.ok) throw new Error('Failed to update charging station group');
    return response.json();
  },

  async deleteChargingStationGroup(id: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/charging-station-groups/${id}`, {
      method: 'DELETE',
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to delete charging station group');
  },

  async addStationToGroup(groupId: string, stationId: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/charging-station-groups/${groupId}/stations/${stationId}`, {
      method: 'POST',
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to add station to group');
  },

  async removeStationFromGroup(groupId: string, stationId: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/charging-station-groups/${groupId}/stations/${stationId}`, {
      method: 'DELETE',
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to remove station from group');
  },

  async getChargingStationGroupDetails(id: string): Promise<any> {
    const response = await fetch(`${API_BASE_URL}/charging-station-groups/${id}`, {
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to fetch charging station group details');
    return response.json();
  },

  // Authorization Methods
  async getAuthorizationMethods(): Promise<any[]> {
    const response = await fetch(`${API_BASE_URL}/authorization-methods`, {
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to fetch authorization methods');
    return response.json();
  },

  async getAuthorizationMethodsByUser(userId: string): Promise<any[]> {
    const response = await fetch(`${API_BASE_URL}/authorization-methods/user/${userId}`, {
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to fetch user authorization methods');
    return response.json();
  },

  async createAuthorizationMethod(method: any): Promise<any> {
    const response = await fetch(`${API_BASE_URL}/authorization-methods`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(method),
    });
    if (!response.ok) throw new Error('Failed to create authorization method');
    return response.json();
  },

  async updateAuthorizationMethod(id: string, method: any): Promise<any> {
    const response = await fetch(`${API_BASE_URL}/authorization-methods/${id}`, {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify(method),
    });
    if (!response.ok) throw new Error('Failed to update authorization method');
    return response.json();
  },

  async deleteAuthorizationMethod(id: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/authorization-methods/${id}`, {
      method: 'DELETE',
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to delete authorization method');
  },

  async verifyAuthorization(type: number, identifier: string): Promise<any> {
    const response = await fetch(`${API_BASE_URL}/authorization-methods/verify`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify({ type, identifier }),
    });
    if (!response.ok) throw new Error('Authorization failed');
    return response.json();
  },

  // User Portal APIs
  async getUserDashboard(): Promise<any> {
    const response = await fetch(`${API_BASE_URL}/user-portal/dashboard`, {
      headers: getAuthHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch user dashboard');
    return response.json();
  },

  async getUserAvailableStations(): Promise<any[]> {
    const response = await fetch(`${API_BASE_URL}/user-portal/available-stations`, {
      headers: getAuthHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch available stations');
    return response.json();
  },

  async getUserChargingSessions(limit?: number): Promise<any[]> {
    const url = limit 
      ? `${API_BASE_URL}/user-portal/charging-sessions?limit=${limit}`
      : `${API_BASE_URL}/user-portal/charging-sessions`;
    const response = await fetch(url, {
      headers: getAuthHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch charging sessions');
    return response.json();
  },

  async getUserCosts(year?: number, month?: number): Promise<any> {
    let url = `${API_BASE_URL}/user-portal/costs`;
    const params = new URLSearchParams();
    if (year) params.append('year', year.toString());
    if (month) params.append('month', month.toString());
    if (params.toString()) url += `?${params.toString()}`;
    
    const response = await fetch(url, {
      headers: getAuthHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch costs');
    return response.json();
  },

  // User Group Charging Station Group Permissions
  async getUserGroupStationPermissions(userGroupId: string): Promise<any[]> {
    const response = await fetch(`${API_BASE_URL}/user-groups/${userGroupId}/station-permissions`, {
      headers: getAuthHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch station permissions');
    return response.json();
  },

  async addStationPermissionToUserGroup(userGroupId: string, chargingStationGroupId: string): Promise<any> {
    const response = await fetch(`${API_BASE_URL}/user-groups/${userGroupId}/station-permissions`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify({ chargingStationGroupId })
    });
    if (!response.ok) throw new Error('Failed to add station permission');
    return response.json();
  },

  async removeStationPermissionFromUserGroup(userGroupId: string, chargingStationGroupId: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/user-groups/${userGroupId}/station-permissions/${chargingStationGroupId}`, {
      method: 'DELETE',
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to remove station permission');
  },

  // Tariffs
  async getTariffs(): Promise<Tariff[]> {
    const response = await fetch(`${API_BASE_URL}/tariffs`, {
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to fetch tariffs');
    return response.json();
  },

  async getTariff(id: string): Promise<Tariff> {
    const response = await fetch(`${API_BASE_URL}/tariffs/${id}`, {
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to fetch tariff');
    return response.json();
  },

  async createTariff(data: CreateTariffRequest): Promise<Tariff> {
    const response = await fetch(`${API_BASE_URL}/tariffs`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(data)
    });
    if (!response.ok) throw new Error('Failed to create tariff');
    return response.json();
  },

  async updateTariff(id: string, data: CreateTariffRequest): Promise<Tariff> {
    const response = await fetch(`${API_BASE_URL}/tariffs/${id}`, {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify(data)
    });
    if (!response.ok) throw new Error('Failed to update tariff');
    return response.json();
  },

  async deleteTariff(id: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/tariffs/${id}`, {
      method: 'DELETE',
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to delete tariff');
  },

  async assignTariffToUserGroup(tariffId: string, userGroupId: string, priority: number): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/tariffs/${tariffId}/usergroups/${userGroupId}`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(priority)
    });
    if (!response.ok) throw new Error('Failed to assign tariff to user group');
  },

  async removeTariffFromUserGroup(tariffId: string, userGroupId: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/tariffs/${tariffId}/usergroups/${userGroupId}`, {
      method: 'DELETE',
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to remove tariff from user group');
  },

  // Session Cost Breakdown
  async getSessionCostBreakdown(sessionId: string): Promise<SessionCostBreakdown> {
    const response = await fetch(`${API_BASE_URL}/user-portal/charging-sessions/${sessionId}/cost-breakdown`, {
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to fetch session cost breakdown');
    return response.json();
  },

  async getBillingSummary(): Promise<BillingSummary> {
    const response = await fetch(`${API_BASE_URL}/billing/summary`, {
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to fetch billing summary');
    return response.json();
  },

  async getBillingAccounts(): Promise<BillingAccount[]> {
    const response = await fetch(`${API_BASE_URL}/billing/accounts`, {
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to fetch billing accounts');
    return response.json();
  },

  async markTransactionAsPaid(transactionId: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/billing/transactions/${transactionId}/mark-paid`, {
      method: 'POST',
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to mark transaction as paid');
  },

  async refundTransaction(transactionId: string, reason: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/billing/transactions/${transactionId}/refund`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify({ reason })
    });
    if (!response.ok) throw new Error('Failed to refund transaction');
  },

  // PDF Export
  async downloadInvoicePdf(transactionId: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/billing/transactions/${transactionId}/pdf`, {
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to download PDF');
    
    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `Rechnung_${transactionId.substring(0, 8)}.pdf`;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
  },

  async downloadMonthlySummaryPdf(year: number, month: number): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/billing/monthly-summary/pdf?year=${year}&month=${month}`, {
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to download PDF');
    
    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `Monatsabrechnung_${year}_${month.toString().padStart(2, '0')}.pdf`;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
  },

  // User Billing
  async getUserBillingTransactions(): Promise<BillingTransaction[]> {
    const response = await fetch(`${API_BASE_URL}/user-portal/billing-transactions`, {
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to fetch user billing transactions');
    return response.json();
  },

  // Vehicle Assignments
  async getVehicleAssignments(includeReturned: boolean = false): Promise<VehicleAssignment[]> {
    const response = await fetch(`${API_BASE_URL}/vehicle-assignments?includeReturned=${includeReturned}`, {
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to fetch vehicle assignments');
    return response.json();
  },

  async getVehicleAssignment(id: string): Promise<VehicleAssignment> {
    const response = await fetch(`${API_BASE_URL}/vehicle-assignments/${id}`, {
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to fetch vehicle assignment');
    return response.json();
  },

  async getVehicleAssignmentsByVehicle(vehicleId: string, includeReturned: boolean = false): Promise<VehicleAssignment[]> {
    const response = await fetch(`${API_BASE_URL}/vehicle-assignments/vehicle/${vehicleId}?includeReturned=${includeReturned}`, {
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to fetch vehicle assignments');
    return response.json();
  },

  async getVehicleAssignmentsByUser(userId: string, includeReturned: boolean = false): Promise<VehicleAssignment[]> {
    const response = await fetch(`${API_BASE_URL}/vehicle-assignments/user/${userId}?includeReturned=${includeReturned}`, {
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to fetch vehicle assignments');
    return response.json();
  },

  async createVehicleAssignment(data: CreateVehicleAssignmentRequest): Promise<VehicleAssignment> {
    const response = await fetch(`${API_BASE_URL}/vehicle-assignments`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(data)
    });
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || 'Failed to create vehicle assignment');
    }
    return response.json();
  },

  async updateVehicleAssignment(id: string, data: UpdateVehicleAssignmentRequest): Promise<VehicleAssignment> {
    const response = await fetch(`${API_BASE_URL}/vehicle-assignments/${id}`, {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify(data)
    });
    if (!response.ok) throw new Error('Failed to update vehicle assignment');
    return response.json();
  },

  async returnVehicle(assignmentId: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/vehicle-assignments/${assignmentId}/return`, {
      method: 'POST',
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to return vehicle');
  },

  async deleteVehicleAssignment(id: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/vehicle-assignments/${id}`, {
      method: 'DELETE',
      headers: getAuthHeaders()
    });
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || 'Failed to delete vehicle assignment');
    }
  },

  // User Portal - My Vehicles
  async getMyVehicles(): Promise<VehicleAssignment[]> {
    // Get current user's vehicles through the assignments endpoint
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    if (!user.id) return [];
    
    return this.getVehicleAssignmentsByUser(user.id, false);
  },

  // Charging Sessions - Start/Stop
  async startChargingSession(connectorId: string, vehicleId?: string): Promise<any> {
    const url = vehicleId 
      ? `${API_BASE_URL}/charging/start/${connectorId}?vehicleId=${vehicleId}`
      : `${API_BASE_URL}/charging/start/${connectorId}`;
    
    const response = await fetch(url, {
      method: 'POST',
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to start charging session');
    return response.json();
  },

  async stopChargingSession(sessionId: string): Promise<any> {
    const response = await fetch(`${API_BASE_URL}/charging/stop/${sessionId}`, {
      method: 'POST',
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to stop charging session');
    return response.json();
  },

  async getActiveSessions(): Promise<any[]> {
    const response = await fetch(`${API_BASE_URL}/charging/sessions/active`, {
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to fetch active sessions');
    return response.json();
  },

  async getStationConnectors(stationId: string): Promise<any[]> {
    const response = await fetch(`${API_BASE_URL}/charging/stations/${stationId}/connectors`, {
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to fetch station connectors');
    return response.json();
  }
};

// Mock data fallbacks
function getMockTenants(): Tenant[] {
  return [
    {
      id: '1',
      name: 'Default Company',
      subdomain: 'default',
      description: 'Default tenant for development',
      isActive: true,
      createdAt: '2024-01-15',
      userCount: 25
    },
    {
      id: '2',
      name: 'Green Energy GmbH',
      subdomain: 'green-energy',
      description: 'Renewable energy company',
      isActive: true,
      createdAt: '2024-02-01',
      userCount: 45
    },
    {
      id: '3',
      name: 'City Parking Solutions',
      subdomain: 'city-parking',
      description: 'Municipal parking management',
      isActive: false,
      createdAt: '2024-01-20',
      userCount: 12
    },
    {
      id: '4',
      name: 'EV Charging Network',
      subdomain: 'ev-network',
      description: 'Public EV charging network',
      isActive: true,
      createdAt: '2024-03-10',
      userCount: 89
    },
    {
      id: '5',
      name: 'Corporate Fleet Solutions',
      subdomain: 'corporate-fleet',
      description: 'Enterprise fleet management',
      isActive: true,
      createdAt: '2024-02-28',
      userCount: 156
    }
  ];
}

function getMockChargingStations(): ChargingStation[] {
  return [
    {
      id: '1',
      stationId: 'CCS-001',
      name: 'CCS Schnellladesäule 1',
      status: 'online',
      latitude: 48.1351,
      longitude: 11.5820,
      maxPower: 150,
      numberOfConnectors: 2,
      vendor: 'Siemens',
      model: 'Sicharge CC',
      type: 'DC',
      createdAt: '2024-01-20T10:00:00Z',
      lastHeartbeat: '2024-01-23T14:30:00Z'
    },
    {
      id: '2',
      stationId: 'AC-001',
      name: 'AC Standardladesäule 1',
      status: 'online',
      latitude: 48.1352,
      longitude: 11.5821,
      maxPower: 22,
      numberOfConnectors: 2,
      vendor: 'ABB',
      model: 'Terra AC',
      type: 'AC',
      createdAt: '2024-01-18T09:00:00Z',
      lastHeartbeat: '2024-01-23T14:25:00Z'
    },
    {
      id: '3',
      stationId: 'CCS-002',
      name: 'CCS Schnellladesäule Berlin',
      status: 'maintenance',
      latitude: 52.5200,
      longitude: 13.4050,
      maxPower: 150,
      numberOfConnectors: 2,
      vendor: 'Siemens',
      model: 'Sicharge CC',
      type: 'DC',
      createdAt: '2024-01-15T11:00:00Z'
    }
  ];
}

function getMockVehicles(): Vehicle[] {
  return [
    {
      id: '1',
      licensePlate: 'M-CC 1234',
      make: 'Tesla',
      model: 'Model 3',
      year: 2023,
      type: 'PoolVehicle',
      color: 'Pearl White',
      isActive: true,
      createdAt: '2024-01-10T08:00:00Z'
    },
    {
      id: '2',
      licensePlate: 'M-CC 5678',
      make: 'BMW',
      model: 'i3',
      year: 2022,
      type: 'CompanyVehicle',
      color: 'Mineral White',
      isActive: true,
      createdAt: '2024-01-15T10:00:00Z'
    }
  ];
}

function getMockChargingSessions(): ChargingSession[] {
  return [
    {
      id: '1',
      user: 'Max Mustermann',
      vehicle: 'Tesla Model 3',
      station: 'CCS Schnellladesäule 1',
      duration: '45 min',
      cost: '€12.50',
      status: 'completed',
      startedAt: '2024-01-23T12:00:00Z'
    },
    {
      id: '2',
      user: 'Anna Schmidt',
      vehicle: 'BMW i3',
      station: 'AC Standardladesäule 1',
      duration: '32 min',
      cost: '€8.90',
      status: 'completed',
      startedAt: '2024-01-23T11:30:00Z'
    },
    {
      id: '3',
      user: 'Peter Müller',
      vehicle: 'VW ID.4',
      station: 'CCS Schnellladesäule Berlin',
      duration: '67 min',
      cost: '€18.75',
      status: 'completed',
      startedAt: '2024-01-23T10:15:00Z'
    }
  ];
}

function getMockBillingTransactions(): BillingTransaction[] {
  return [
    {
      id: '1',
      amount: 12.50,
      currency: 'EUR',
      description: 'Ladevorgang CCS-001',
      status: 'completed',
      transactionType: 'charging',
      createdAt: '2024-01-23T12:45:00Z',
      account: {
        id: '1',
        accountName: 'Max Mustermann'
      },
      session: {
        id: '1',
        sessionId: 'SESSION-001',
        energyDelivered: 25.5,
        user: 'Max Mustermann',
        station: 'CCS-001'
      }
    },
    {
      id: '2',
      amount: 8.90,
      currency: 'EUR',
      description: 'Ladevorgang AC-001',
      status: 'completed',
      transactionType: 'charging',
      createdAt: '2024-01-23T12:02:00Z',
      account: {
        id: '2',
        accountName: 'Anna Schmidt'
      },
      session: {
        id: '2',
        sessionId: 'SESSION-002',
        energyDelivered: 18.3,
        user: 'Anna Schmidt',
        station: 'AC-001'
      }
    },
    {
      id: '3',
      amount: 18.75,
      currency: 'EUR',
      description: 'Ladevorgang CCS-002',
      status: 'completed',
      transactionType: 'charging',
      createdAt: '2024-01-23T11:22:00Z',
      account: {
        id: '3',
        accountName: 'Peter Müller'
      },
      session: {
        id: '3',
        sessionId: 'SESSION-003',
        energyDelivered: 35.2,
        user: 'Peter Müller',
        station: 'CCS-002'
      }
    }
  ];
}

function getMockDashboardStats(): DashboardStats {
  return {
    totalStations: 3,
    totalVehicles: 2,
    totalTransactions: 15,
    activeStations: 2,
    activeVehicles: 2
  };
}


