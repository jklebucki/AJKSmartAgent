#!/bin/sh
set -eu

validate_credential() {
  value="$1"
  minimum_length="$2"
  label="$3"

  if [ "${#value}" -lt "$minimum_length" ] || ! printf '%s' "$value" | grep -Eq '^[A-Za-z0-9._~+/-]+$'; then
    echo "$label must contain at least $minimum_length URL-safe characters." >&2
    exit 1
  fi
}

validate_credential "$SEAWEEDFS_S3_ACCESS_KEY" 16 "SEAWEEDFS_S3_ACCESS_KEY"
validate_credential "$SEAWEEDFS_S3_SECRET_KEY" 32 "SEAWEEDFS_S3_SECRET_KEY"

umask 077
cat > /run/seaweedfs/s3.json <<EOF
{
  "identities": [
    {
      "name": "praxiara",
      "credentials": [
        {
          "accessKey": "$SEAWEEDFS_S3_ACCESS_KEY",
          "secretKey": "$SEAWEEDFS_S3_SECRET_KEY"
        }
      ],
      "actions": ["Admin", "Read", "List", "Tagging", "Write"]
    }
  ]
}
EOF

exec weed server \
  -dir=/data \
  -ip=seaweedfs \
  -ip.bind=0.0.0.0 \
  -master.port=9333 \
  -master.telemetry=false \
  -master.volumeSizeLimitMB="$SEAWEEDFS_VOLUME_SIZE_LIMIT_MB" \
  -volume.port=8080 \
  -volume.max="$SEAWEEDFS_VOLUME_MAX" \
  -filer.port=8888 \
  -metricsPort=9327 \
  -s3 \
  -s3.port=8333 \
  -s3.config=/run/seaweedfs/s3.json \
  -s3.allowedOrigins="$SEAWEEDFS_S3_ALLOWED_ORIGINS" \
  -s3.allowDeleteBucketNotEmpty=false
