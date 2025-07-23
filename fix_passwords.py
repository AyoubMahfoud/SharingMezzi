import sqlite3
import hashlib
import base64

def hash_password(password):
    """Hash password using SHA256 like in the AuthService"""
    sha256_hash = hashlib.sha256(password.encode('utf-8')).digest()
    return base64.b64encode(sha256_hash).decode('utf-8')

def fix_passwords():
    """Fix passwords in the database"""
    conn = sqlite3.connect('SharingMezzi.Api/sharingmezzi.db')
    cursor = conn.cursor()
    
    # Check current passwords
    cursor.execute("SELECT Id, Email, Password FROM Utenti")
    users = cursor.fetchall()
    
    print("Current users:")
    for user in users:
        print(f"ID: {user[0]}, Email: {user[1]}, Password: {user[2]}")
    
    # Update passwords
    passwords = {
        'admin@test.com': 'admin123',
        'mario@test.com': 'password123',
        'lucia@test.com': 'password123'
    }
    
    for email, password in passwords.items():
        hashed_password = hash_password(password)
        cursor.execute("UPDATE Utenti SET Password = ? WHERE Email = ?", (hashed_password, email))
        print(f"Updated password for {email}: {hashed_password}")
    
    conn.commit()
    conn.close()
    print("Passwords updated successfully!")

if __name__ == "__main__":
    fix_passwords()
