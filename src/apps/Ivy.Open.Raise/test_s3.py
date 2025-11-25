import boto3
from botocore.config import Config
import sys
sys.stdout.reconfigure(encoding='utf-8')

# SeaweedFS S3 configuration from your user secrets
ENDPOINT = "http://fs-open-raise-2.sliplane.app:8333"
ACCESS_KEY = "uumsoEtZJHy7s6R6GUXw"
SECRET_KEY = "l625oxExyuFbcokRf1FGc24KVTUlmuwlrlVaKNDV7iOXCtj7mmkuDAkMypmFvjo"

BUCKET = "decks"
TEST_KEY = "43436dae-e82a-4ba5-a2a8-0f9f3dbd5e62.pdf"

def main():
    print(f"Connecting to: {ENDPOINT}")
    print(f"Access Key: {ACCESS_KEY[:8]}...")
    print()

    # Create S3 client
    s3 = boto3.client(
        's3',
        endpoint_url=ENDPOINT,
        aws_access_key_id=ACCESS_KEY,
        aws_secret_access_key=SECRET_KEY,
        config=Config(signature_version='s3v4'),
        region_name='us-east-1'
    )

    # Test 1: List buckets
    print("=" * 50)
    print("TEST 1: List Buckets")
    print("=" * 50)
    try:
        response = s3.list_buckets()
        print("Buckets:")
        for bucket in response['Buckets']:
            print(f"  - {bucket['Name']}")
        print("✓ List buckets: SUCCESS")
    except Exception as e:
        print(f"✗ List buckets: FAILED - {e}")
    print()

    # Test 2: List objects in bucket
    print("=" * 50)
    print(f"TEST 2: List Objects in '{BUCKET}'")
    print("=" * 50)
    try:
        response = s3.list_objects_v2(Bucket=BUCKET, MaxKeys=5)
        print("Objects (first 5):")
        for obj in response.get('Contents', []):
            print(f"  - {obj['Key']} ({obj['Size']} bytes)")
        print("✓ List objects: SUCCESS")
    except Exception as e:
        print(f"✗ List objects: FAILED - {e}")
    print()

    # Test 3: Check if specific file exists
    print("=" * 50)
    print(f"TEST 3: Head Object '{TEST_KEY}'")
    print("=" * 50)
    try:
        response = s3.head_object(Bucket=BUCKET, Key=TEST_KEY)
        print(f"  Content-Type: {response.get('ContentType')}")
        print(f"  Size: {response.get('ContentLength')} bytes")
        print(f"  Last Modified: {response.get('LastModified')}")
        print("✓ Head object: SUCCESS")
    except Exception as e:
        print(f"✗ Head object: FAILED - {e}")
    print()

    # Test 4: Generate pre-signed URL (HTTP)
    print("=" * 50)
    print("TEST 4: Generate Pre-signed URL")
    print("=" * 50)
    try:
        url = s3.generate_presigned_url(
            'get_object',
            Params={'Bucket': BUCKET, 'Key': TEST_KEY},
            ExpiresIn=3600
        )
        print(f"Generated URL:\n{url}")
        print()

        # Check the protocol
        if url.startswith("http://"):
            print("✓ Protocol: HTTP (correct)")
        elif url.startswith("https://"):
            print("⚠ Protocol: HTTPS (may cause SSL error if server doesn't support it)")
        print("✓ Generate pre-signed URL: SUCCESS")
    except Exception as e:
        print(f"✗ Generate pre-signed URL: FAILED - {e}")
    print()

    # Test 5: Actually try to download the file
    print("=" * 50)
    print("TEST 5: Download Object (first 1KB)")
    print("=" * 50)
    try:
        response = s3.get_object(Bucket=BUCKET, Key=TEST_KEY, Range='bytes=0-1023')
        data = response['Body'].read()
        print(f"  Downloaded {len(data)} bytes")
        print(f"  Content-Type: {response.get('ContentType')}")
        print("✓ Download: SUCCESS")
    except Exception as e:
        print(f"✗ Download: FAILED - {e}")
    print()

    # Test 6: Try fetching the pre-signed URL directly
    print("=" * 50)
    print("TEST 6: Fetch Pre-signed URL via HTTP")
    print("=" * 50)
    try:
        import urllib.request
        import urllib.error
        url = s3.generate_presigned_url(
            'get_object',
            Params={'Bucket': BUCKET, 'Key': TEST_KEY},
            ExpiresIn=3600
        )
        print(f"  URL: {url[:80]}...")
        req = urllib.request.Request(url, method='GET')
        req.add_header('User-Agent', 'Mozilla/5.0')
        with urllib.request.urlopen(req, timeout=10) as resp:
            print(f"  Status: {resp.status}")
            print(f"  Content-Type: {resp.headers.get('Content-Type')}")
            print(f"  Content-Length: {resp.headers.get('Content-Length')}")
        print("✓ Fetch pre-signed URL: SUCCESS")
    except urllib.error.HTTPError as e:
        print(f"✗ Fetch pre-signed URL: FAILED - HTTP {e.code}")
        print(f"  Response body: {e.read().decode('utf-8', errors='ignore')}")
    except Exception as e:
        print(f"✗ Fetch pre-signed URL: FAILED - {e}")

    # Test 7: Check if public access works (unsigned)
    print()
    print("=" * 50)
    print("TEST 7: Direct Access (no signature)")
    print("=" * 50)
    try:
        direct_url = f"{ENDPOINT}/{BUCKET}/{TEST_KEY}"
        print(f"  URL: {direct_url}")
        req = urllib.request.Request(direct_url, method='HEAD')
        with urllib.request.urlopen(req, timeout=10) as resp:
            print(f"  Status: {resp.status}")
        print("✓ Direct access: SUCCESS (bucket is public)")
    except urllib.error.HTTPError as e:
        print(f"  Direct access returned: HTTP {e.code}")
        if e.code == 403:
            print("  (Expected - bucket requires authentication)")
    except Exception as e:
        print(f"  Direct access: {e}")

if __name__ == "__main__":
    main()
