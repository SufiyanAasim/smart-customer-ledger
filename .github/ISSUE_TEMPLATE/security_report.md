name: Security Report
description: Report a security vulnerability confidentially.
title: "[SECURITY] "
labels: ["security"]
body:
  - type: textarea
    id: details
    attributes:
      label: Vulnerability Details
      description: Provide details of the security issue.
    validations:
      required: true
