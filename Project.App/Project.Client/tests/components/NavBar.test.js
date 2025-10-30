import { render, screen, waitFor, act } from '@testing-library/react';
import NavBar from '@/app/components/NavBar';

// Mock Next.js navigation hooks
jest.mock('next/navigation', () => ({
  usePathname: jest.fn(() => '/'),
}));

// Mock fetch for user data
global.fetch = jest.fn();

// Mock console.error to suppress expected error messages
const originalConsoleError = console.error;
beforeAll(() => {
  console.error = jest.fn();
});

afterAll(() => {
  console.error = originalConsoleError;
});

describe('NavBar', () => {
  beforeEach(() => {
    fetch.mockClear();
    // Default mock for fetch to avoid errors
    fetch.mockImplementation(() => Promise.resolve({
      ok: false,
      status: 401,
    }));
  });

  it('should render without crashing', async () => {
    const { usePathname } = require('next/navigation');
    usePathname.mockReturnValue('/');
    
    await act(async () => {
      render(<NavBar />);
    });
    
    expect(screen.getByText('Double Down Devito')).toBeInTheDocument();
  });

  it('should show Rooms link on player pages', async () => {
    const { usePathname } = require('next/navigation');
    usePathname.mockReturnValue('/player/123');
    
    await act(async () => {
      render(<NavBar />);
    });
    
    expect(screen.getByText('Rooms')).toBeInTheDocument();
    expect(screen.getByText('Rooms')).toHaveAttribute('href', '/rooms');
  });

  it('should show Profile link with placeholder on rooms page while loading', async () => {
    const { usePathname } = require('next/navigation');
    usePathname.mockReturnValue('/rooms');
    
    // Mock fetch to delay response
    fetch.mockImplementation(() => 
      new Promise(() => {}) // Never resolves to simulate loading
    );
    
    await act(async () => {
      render(<NavBar />);
    });
    
    // Profile link should be visible immediately with placeholder
    const profileLink = screen.getByText('Profile');
    expect(profileLink).toBeInTheDocument();
    expect(profileLink).toHaveAttribute('href', '#');
  });

  it('should update Profile link with correct user ID after loading', async () => {
    const { usePathname } = require('next/navigation');
    usePathname.mockReturnValue('/rooms');
    
    const mockUserId = 'test-user-123';
    const mockUserData = { id: mockUserId, name: 'Test User', email: 'test@example.com', balance: 1000 };
    
    // Mock successful fetch response
    fetch.mockResolvedValueOnce({
      ok: true,
      json: async () => mockUserData,
    });
    
    await act(async () => {
      render(<NavBar />);
    });
    
    // Wait for the Profile link to update with the correct user ID
    await waitFor(() => {
      const profileLink = screen.getByText('Profile');
      expect(profileLink).toHaveAttribute('href', `/player/${mockUserId}`);
    });
  });

  it('should not show navigation links on game pages', async () => {
    const { usePathname } = require('next/navigation');
    usePathname.mockReturnValue('/game/456');
    
    await act(async () => {
      render(<NavBar />);
    });
    
    // Should not show Rooms or Profile links
    expect(screen.queryByText('Rooms')).not.toBeInTheDocument();
    expect(screen.queryByText('Profile')).not.toBeInTheDocument();
  });

  it('should show logout button on protected pages', async () => {
    const { usePathname } = require('next/navigation');
    usePathname.mockReturnValue('/rooms');
    
    await act(async () => {
      render(<NavBar />);
    });
    
    expect(screen.getByText('Logout')).toBeInTheDocument();
  });

  it('should not show logout button on non-protected pages', async () => {
    const { usePathname } = require('next/navigation');
    usePathname.mockReturnValue('/login');
    
    await act(async () => {
      render(<NavBar />);
    });
    
    expect(screen.queryByText('Logout')).not.toBeInTheDocument();
  });
});
